﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Logging;
using Editor.Extensions;
using Editor.Models;
using Editor.Models.RSI;
using Octokit;
using SpaceWizards.RsiLib.Directions;
using SpaceWizards.RsiLib.DMI.Metadata;
using SpaceWizards.RsiLib.RSI;
using ReactiveUI;
using SixLabors.ImageSharp.PixelFormats;
using Splat;
using Application = Avalonia.Application;
using Image = SixLabors.ImageSharp.Image;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Editor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const string StatesClipboard = "RSIEdit_States";

    private static readonly HttpClient Http = new();
    private static readonly GitHubClient GitHub = new(new ProductHeaderValue("RSIEdit"));

    private static readonly ImmutableArray<string> ValidDownloadHosts =
        ImmutableArray.Create<string>("github.com", "www.github.com");

    private int _selectedIndex;
    private RsiItemViewModel? _currentOpenRsi;
    private readonly ObservableCollection<RsiItemViewModel> _openRsis = new();
    private string? _lastOpenedElement;
    private bool _hasLastOpenedElement;
    private bool _isRsiOpen;
    private bool _hasCopiedStates;
    private readonly List<RsiImage> _copiedStates = new();
    private string? _copiedLicense;
    private string? _copiedCopyright;

    public Preferences Preferences { get; } = Locator.Current.GetRequiredService<Preferences>();

    private RepositoryLicenses RepositoryLicenses { get; } = Locator.Current.GetRequiredService<RepositoryLicenses>();

    private Task<RepositoryLicenses?> OnlineRepositoryLicenses { get; } = Locator.Current.GetRequiredService<Task<RepositoryLicenses?>>();

    private MetadataParser DmiParser { get; } = new();

    private string? LastOpenedElement
    {
        get => _lastOpenedElement;
        set
        {
            this.RaiseAndSetIfChanged(ref _lastOpenedElement, value);
            HasLastOpenedElement = value != null;
        }
    }

    public bool HasLastOpenedElement
    {
        get => _hasLastOpenedElement;
        set => this.RaiseAndSetIfChanged(ref _hasLastOpenedElement, value);
    }

    public IReadOnlyList<RsiItemViewModel> OpenRsis => _openRsis;

    public RsiItemViewModel? CurrentOpenRsi
    {
        get => _currentOpenRsi;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentOpenRsi, value);

            if (value != null)
            {
                SelectedIndex = _openRsis.IndexOf(value);
            }
        }
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedIndex, value);

            if (value >= 0)
            {
                _currentOpenRsi = _openRsis[value];
            }
        }
    }

    public bool IsRsiOpen
    {
        get => _isRsiOpen;
        set => this.RaiseAndSetIfChanged(ref _isRsiOpen, value);
    }

    public Interaction<Unit, bool> NewRsiAction { get; } = new();

    public Interaction<Unit, string?> OpenRsiDialog { get; } = new();

    public Interaction<Unit, string?> SaveRsiDialog { get; } = new();

    public Interaction<Unit, string?> ImportImageDialog { get; } = new();

    public Interaction<Unit, string?> ImportDmiDialog { get; } = new();

    public Interaction<Unit, string?> ImportDmiFolderDialog { get; } = new();

    public Interaction<Unit, Unit> PreferencesAction { get; } = new();

    public Interaction<ErrorWindowViewModel, Unit> ErrorDialog { get; } = new();

    public Interaction<Unit, string?> ChangeAllLicensesAction { get; } = new();

    public Interaction<Unit, string?> ChangeAllCopyrightsAction { get; } = new();

    public void Reset()
    {
        foreach (var rsi in _openRsis.ToArray())
        {
            CloseRsi(rsi);
        }

        CurrentOpenRsi = null;
        LastOpenedElement = null;
    }

    private void AddRsi(RsiItemViewModel vm)
    {
        _openRsis.Add(vm);
        CurrentOpenRsi = vm;
        IsRsiOpen = true;
    }

    public void CloseRsi(RsiItemViewModel vm)
    {
        if (_openRsis.Remove(vm) && CurrentOpenRsi == vm)
        {
            if (_openRsis.Count == 0)
            {
                CurrentOpenRsi = null;
                IsRsiOpen = false;
            }
            else
            {
                CurrentOpenRsi = _openRsis[^1];
            }
        }
    }

    #region File

    public async void New()
    {
        await NewAsync();
    }

    public async Task NewAsync()
    {
        if (await NewRsiAction.Handle(Unit.Default))
            CreateNewRsi();
    }

    private void CreateNewRsi(string? title = null, RsiItem? rsi = null)
    {
        AddRsi(new RsiItemViewModel(title, rsi)
        {
            License = rsi?.Rsi.License ?? Preferences.DefaultLicense,
            Copyright = rsi?.Rsi.Copyright ?? Preferences.DefaultCopyright,
        });
    }

    public async Task OpenRsi(string folderPath)
    {
        var metaJsonFiles = Directory.GetFiles(folderPath, "meta.json");

        if (metaJsonFiles.Length == 0)
        {
            await ErrorDialog.Handle(new ErrorWindowViewModel($"No meta.json found in folder\n{folderPath}\n\nIs it an RSI?"));
            return;
        }

        if (metaJsonFiles.Length > 1)
        {
            await ErrorDialog.Handle(new ErrorWindowViewModel($"More than one meta.json found in folder:\n{folderPath}"));
            return;
        }

        Rsi rsi;
        await using (var stream = File.OpenRead(metaJsonFiles[0]))
        {
            rsi = Rsi.FromMetaJson(stream);
        }

        // ReSharper disable once RedundantAlwaysMatchSubpattern
        if (rsi is not {Size: {}})
        {
            await ErrorDialog.Handle(new ErrorWindowViewModel("Error loading meta.json:\nMissing size property."));
            return;
        }

        try
        {
            rsi.TryLoadFolderImages(folderPath);
        }
        catch (Exception e)
        {
            Logger.Sink?.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
            await ErrorDialog.Handle(new ErrorWindowViewModel($"Error loading .rsi images:\n{e.Message}"));
            return;
        }

        var rsiItem = new RsiItem(rsi);
        var name = Path.GetFileName(folderPath);

        var license = string.IsNullOrEmpty(rsiItem.Rsi.License) ? Preferences.DefaultLicense : rsiItem.Rsi.License;
        var copyright = string.IsNullOrEmpty(rsiItem.Rsi.Copyright)
            ? Preferences.DefaultCopyright
            : rsiItem.Rsi.Copyright;

        var rsiVm = new RsiItemViewModel(name, rsiItem)
        {
            SaveFolder = folderPath,
            License = license,
            Copyright = copyright,
        };

        AddRsi(rsiVm);
        LastOpenedElement = folderPath;
    }

    public async void Open()
    {
        var folder = await OpenRsiDialog.Handle(Unit.Default);
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        await OpenRsi(folder);
    }

    public void Save()
    {
        if (CurrentOpenRsi == null)
        {
            return;
        }

        if (CurrentOpenRsi.SaveFolder == null)
        {
            SaveAs();
            return;
        }

        CurrentOpenRsi.Save();
    }

    public void SaveAs()
    {
        if (CurrentOpenRsi == null)
        {
            return;
        }

        SaveRsiAs(CurrentOpenRsi);
    }

    public async void SaveAll()
    {
        var path = await SaveRsiDialog.Handle(Unit.Default);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        foreach (var rsi in _openRsis)
        {
            rsi.SaveFolder = Path.Combine(path, $"{rsi.Title}");
            SaveRsi(rsi);
        }
    }

    public void Close()
    {
        if (CurrentOpenRsi != null)
            CloseRsi(CurrentOpenRsi);
    }

    public async Task Paste()
    {
        if (Application.Current?.Clipboard is not { } clipboard)
            return;

        var text = await clipboard.GetTextAsync();
        if (text == StatesClipboard)
        {
            await PasteStates();
            return;
        }

        if (string.IsNullOrWhiteSpace(text) ||
            !Uri.TryCreate(text, UriKind.Absolute, out var url) ||
            !ValidDownloadHosts.Contains(url.Host))
        {
            return;
        }

        var builder = new UriBuilder(url) { Port = -1 };
        var pathStrings = builder.Path.Split('/');
        if (pathStrings.Length < 4)
            return;

        pathStrings[3] = "raw";

        var dmiExtensionIndex = pathStrings[^1].LastIndexOf(".dmi", StringComparison.Ordinal);
        if (dmiExtensionIndex == -1)
            return;

        builder.Path = string.Join('/', pathStrings);

        var owner = pathStrings[1];
        var codebase = pathStrings[2];
        var filePath = string.Join("/", pathStrings[5..]);
        var usingApi = false;
        Stream stream;
        if (!string.IsNullOrWhiteSpace(Preferences.GitHubToken))
        {
            GitHub.Credentials = new Credentials(token: Preferences.GitHubToken);
            var content = await GitHub.Repository.Content.GetRawContent(owner, codebase, filePath);
        
            stream = new MemoryStream();
            stream.Write(content);
            stream.Position = 0;
            usingApi = true;
        }
        else
        {
            var link = builder.ToString();
            var downloadResponse = await Http.GetAsync(link);
            if (!downloadResponse.IsSuccessStatusCode)
                return;

            if (downloadResponse.RequestMessage?.RequestUri is { } uri &&
                uri.Host == "github.com" &&
                uri.AbsolutePath == "/login")
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel($"The given GitHub link requires an access token to access.\n" +
                                                                  $"See preferences for more details.\n" +
                                                                  $"If you are already using a token, check that it has access to the pasted repository:\n{link}"));
                return;
            }

            stream = await downloadResponse.Content.ReadAsStreamAsync();
        }
        
        var rsi = await LoadDmi(stream, text);
        if (rsi == null)
            return;

        pathStrings[3] = "commits";

        builder.Path = string.Join('/', pathStrings);

        string? commitHash = null;
        if (usingApi)
        {
            var commits = await GitHub.Repository.Commit.GetAll(owner, codebase, new CommitRequest { Path = filePath });
            commitHash = commits.FirstOrDefault()?.Sha;
        }
        else
        {
            var response = await Http.GetAsync(builder.ToString());
            if (!response.IsSuccessStatusCode)
                return;

            var html = await response.Content.ReadAsStringAsync();

            // We aren't trying to match tags so we should be safe from Zalgo
            var lastCommitRegex = new Regex($"\"/{owner}/{codebase}/commit/([a-zA-Z0-9]+)", RegexOptions.None, TimeSpan.FromSeconds(10));
            var match = lastCommitRegex.Match(html);
            if (match.Success)
                commitHash = match.Groups[1].Value;
        }

        if (commitHash != null)
        {
            pathStrings[3] = "blob";
            pathStrings[4] = commitHash;

            builder.Path = string.Join('/', pathStrings);

            var link = builder.ToString();
            rsi.Copyright = $"Taken from {codebase} at {link}";

            var path = builder.Path;
            if (path.StartsWith('/'))
                path = path[1..];

            var repositories = RepositoryLicenses.RepositoriesRegex;
            var useLocalLicenses = Environment.GetEnvironmentVariable("USE_LOCAL_LICENSES");
            if (useLocalLicenses is null or "0" &&
                OnlineRepositoryLicenses.IsCompletedSuccessfully &&
                (await OnlineRepositoryLicenses)?.RepositoriesRegex is { } onlineRepositories)
            {
                repositories = onlineRepositories;
            }

            foreach (var (repo, license) in repositories)
            {
                if (repo.IsMatch(path))
                {
                    rsi.License = license;
                    break;
                }
            }
        }

        var dmiName = pathStrings[^1];
        var rsiName = $"{dmiName[..dmiExtensionIndex]}.rsi";

        CreateNewRsi(rsiName, new RsiItem(rsi));
        ReformatAllNames();
    }

    public async Task ImportImage(string filePath)
    {
        Rsi? rsi;

        if (filePath.EndsWith(".dmi"))
        {
            rsi = await LoadDmi(filePath);
        }
        else
        {
            rsi = await LoadImage(filePath);
        }

        if (rsi == null) return;

        var rsiItem = new RsiItem(rsi);
        var name = Path.GetFileNameWithoutExtension(filePath);
        var rsiVm = new RsiItemViewModel(name, rsiItem)
        {
            SaveFolder = null,
            License = Preferences.DefaultLicense,
            Copyright = Preferences.DefaultCopyright
        };

        AddRsi(rsiVm);
        LastOpenedElement = filePath;
    }

    public async void SingleConvertDMI()
    {
        var file = await ImportDmiDialog.Handle(Unit.Default);
        if (string.IsNullOrEmpty(file))
        {
            return;
        }

        var targetPath = Path.ChangeExtension(file, "rsi");

        if (Directory.Exists(targetPath))
        {
            await ErrorDialog.Handle(new ErrorWindowViewModel($"Error converting dmi image as target path {targetPath} already exists"));
            return;
        }

        var rsi = await LoadDmi(file);

        if (rsi == null) return;

        rsi.SaveToFolder(targetPath);
    }

    public async void BulkConvertDMI()
    {
        var directory = await ImportDmiFolderDialog.Handle(Unit.Default);

        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        if (!Directory.Exists(directory))
        {
            await ErrorDialog.Handle(new ErrorWindowViewModel($"{directory} is not a directory"));
        }

        var rsis = new List<(Rsi Rsi, string Path)>();

        await LoadRSIs(directory, rsis);

        foreach (var fn in Directory.GetDirectories(directory, "*", SearchOption.AllDirectories))
        {
            await LoadRSIs(fn, rsis);
        }

        if (rsis.Count == 0) return;

        foreach (var (rsi, path) in rsis)
        {
            rsi.SaveToFolder(path);
        }

        Logger.Sink?.Log(LogEventLevel.Information, "MAIN", null, $"Converted {rsis.Count} DMIs to RSIs");
    }

    public async void Import()
    {
        var file = await ImportImageDialog.Handle(Unit.Default);
        if (string.IsNullOrEmpty(file))
        {
            return;
        }

        await ImportImage(file);
    }

    public async void ReOpenLast()
    {
        if (LastOpenedElement != null)
        {
            if (File.GetAttributes(LastOpenedElement).HasFlag(FileAttributes.Directory))
            {
                await OpenRsi(LastOpenedElement);
            }
            else
            {
                await ImportImage(LastOpenedElement);
            }
        }
    }

    public void OpenCurrentRsi()
    {
        if (CurrentOpenRsi != null && CurrentOpenRsi.SaveFolder != null)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer", CurrentOpenRsi.SaveFolder);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", CurrentOpenRsi.SaveFolder);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", CurrentOpenRsi.SaveFolder);
            }
        }
    }

    public async void OpenPreferences()
    {
        await PreferencesAction.Handle(Unit.Default);
    }

    #endregion

    #region Edit

    public async Task CopyStates()
    {
        if (CurrentOpenRsi?.SelectedStates.Count > 0)
        {
            if (Application.Current?.Clipboard is { } clipboard)
                await clipboard.SetTextAsync(StatesClipboard);

            _copiedStates.Clear();
            foreach (var state in CurrentOpenRsi.SelectedStates)
            {
                var copiedState = new RsiState(state.Image.State);
                _copiedStates.Add(new RsiImage(copiedState, state.Image.Preview));
            }

            _copiedLicense = CurrentOpenRsi.License;
            _copiedCopyright = CurrentOpenRsi.Copyright;
        }
    }

    private async Task PasteStates()
    {
        if (CurrentOpenRsi != null)
        {
            foreach (var copy in _copiedStates)
            {
                var copiedState = new RsiState(copy.State);
                await CurrentOpenRsi.CreateNewState(new RsiImage(copiedState, copy.Preview));
            }

            if (_copiedLicense != null && string.IsNullOrWhiteSpace(CurrentOpenRsi.License))
            {
                CurrentOpenRsi.License = _copiedLicense;
            }

            if (_copiedCopyright != null)
            {
                if (string.IsNullOrWhiteSpace(CurrentOpenRsi.Copyright))
                {
                    CurrentOpenRsi.Copyright = _copiedCopyright;
                }
                else if (!CurrentOpenRsi.Copyright.Contains(_copiedCopyright))
                {
                    var copyright = _copiedCopyright;
                    if (copyright.StartsWith("Taken from "))
                    {
                        var index = copyright.IndexOf(" at ", StringComparison.Ordinal);
                        copyright = copyright[(index + 4)..];
                    }

                    CurrentOpenRsi.Copyright += $", {copyright}";
                }
            }
        }
    }

    public void Undo()
    {
        if (CurrentOpenRsi != null &&
            CurrentOpenRsi.TryRestore(out var selected))
        {
            CurrentOpenRsi.SelectedStates.Add(selected);
        }
    }

    public void UndoAll()
    {
        if (CurrentOpenRsi == null)
        {
            return;
        }

        while (CurrentOpenRsi.TryRestore(out var selected))
        {
            CurrentOpenRsi.SelectedStates.Add(selected);
        }
    }

    public void DeselectAll()
    {
        if (CurrentOpenRsi == null)
        {
            return;
        }

        CurrentOpenRsi.SelectedStates.Clear();
    }

    public void Redo()
    {
        if (CurrentOpenRsi != null &&
            CurrentOpenRsi.TryRedoDelete(out var index))
        {
            CurrentOpenRsi.ReselectState(index);
        }
    }

    public void Directions(DirectionType directions)
    {
        if (CurrentOpenRsi != null && CurrentOpenRsi.SelectedStates.Count != 0)
        {
            foreach (var state in CurrentOpenRsi.SelectedStates)
            {
                state.Image.State.Directions = directions;
            }
        }
    }

    public async void ChangeAllLicenses()
    {
        var license = await ChangeAllLicensesAction.Handle(Unit.Default);
        if (license == null)
        {
            return;
        }

        foreach (var rsi in _openRsis)
        {
            rsi.License = license;
        }
    }

    public async void ChangeAllCopyrights()
    {
        var copyright = await ChangeAllCopyrightsAction.Handle(Unit.Default);
        if (copyright == null)
        {
            return;
        }

        foreach (var rsi in _openRsis)
        {
            rsi.Copyright = copyright;
        }
    }

    public void ReformatAllNames()
    {
        if (CurrentOpenRsi == null)
        {
            return;
        }

        foreach (var state in CurrentOpenRsi.States)
        {
            state.Name = state.Name.Replace(' ', '_').ToLowerInvariant();
        }
    }

    public void SortAlphabetically()
    {
        if (CurrentOpenRsi == null)
        {
            return;
        }

        var states = CurrentOpenRsi.States.ToList();
        states.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        CurrentOpenRsi.States = new ObservableCollection<RsiStateViewModel>(states);
    }

    public void Delete()
    {
        CurrentOpenRsi?.DeleteSelectedStates();
    }

    #endregion

    private async Task LoadRSIs(string directory, List<(Rsi, string)> rsis)
    {
        foreach (var (dmiPath, rsiPath) in GetDMIFiles(directory))
        {
            var rsi = await LoadDmi(dmiPath);

            if (rsi == null) continue;

            rsis.Add((rsi, rsiPath));
        }
    }

    private IEnumerable<(string DMI, string RSI)> GetDMIFiles(string directory)
    {
        foreach (var fn in Directory.GetFiles(directory, "*.dmi", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(fn) != ".dmi") continue;

            var targetPath = Path.ChangeExtension(fn, "rsi");

            if (Directory.Exists(targetPath)) continue;

            yield return (fn, targetPath);
        }
    }

    private async void SaveRsiAs(RsiItemViewModel rsi)
    {
        var path = await SaveRsiDialog.Handle(Unit.Default);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        rsi.SaveFolder = path;
        rsi.Title = Path.GetFileName(path);
        rsi.Save();
    }

    private async Task<Rsi?> LoadImage(string filePath)
    {
        Rsi? rsi = null;
        try
        {
            var image = Image.Load<Rgba32>(filePath);
            rsi = new Rsi(x: image.Width, y: image.Height);
            var state = new RsiState();

            if (filePath.EndsWith(".gif"))
            {
                state.LoadGif(image);
            }
            else if (filePath.EndsWith(".png"))
            {
                state.LoadImage(image, rsi.Size);
            }
            else
            {
                throw new InvalidOperationException($"Invalid file type specified!");
            }

            rsi.States.Add(state);
        }
        catch (Exception e)
        {
            Logger.Sink?.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
            await ErrorDialog.Handle(new ErrorWindowViewModel($"Error loading image\n{filePath}\n{e.Message}"));
        }

        return rsi;
    }

    private async Task<Rsi?> LoadDmi(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        return await LoadDmi(stream, filePath);
    }

    private async Task<Rsi?> LoadDmi(Stream stream, string name)
    {
        try
        {
            if (!DmiParser.TryGetFileMetadata(stream, out var metadata, out var parseError))
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel($"Error loading metadata for dmi\n{name}\n{parseError.Message}"));
                return null;
            }
            
            stream.Seek(0, SeekOrigin.Begin);
            var dmi = Image.Load<Rgba32>(stream);
            return metadata.ToRsi(dmi);
        }
        catch (Exception e)
        {
            Logger.Sink?.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
            await ErrorDialog.Handle(new ErrorWindowViewModel($"Error loading image for dmi\n{name}\n{e.Message}"));
            return null;
        }
    }

    private void SaveRsi(RsiItemViewModel rsi)
    {
        if (rsi.SaveFolder == null)
        {
            SaveAs();
            return;
        }

        rsi.Save();
    }
}
