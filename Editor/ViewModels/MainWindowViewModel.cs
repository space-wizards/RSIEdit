using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Logging;
using Editor.Extensions;
using Editor.Models;
using Editor.Models.RSI;
using Importer.Directions;
using Importer.DMI.Metadata;
using Importer.RSI;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Splat;

namespace Editor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly PngDecoder PngDecoder = new();

        private RsiItemViewModel? _currentOpenRsi;
        private readonly ObservableCollection<RsiItemViewModel> _openRsis = new();

        public MainWindowViewModel()
        {
            Preferences = Locator.Current.GetRequiredService<Preferences>();
        }

        public Preferences Preferences { get; }

        private MetadataParser DmiParser { get; } = new();

        private string? LastOpenedElement { get; set; }

        public IReadOnlyList<RsiItemViewModel> OpenRsis => _openRsis;

        public RsiItemViewModel? CurrentOpenRsi
        {
            get => _currentOpenRsi;
            set => this.RaiseAndSetIfChanged(ref _currentOpenRsi, value);
        }
        
        public Interaction<Unit, bool> NewRsiAction { get; } = new();

        public Interaction<Unit, string> OpenRsiDialog { get; } = new();

        public Interaction<Unit, string> SaveRsiDialog { get; } = new();

        public Interaction<Unit, string> ImportDmiDialog { get; } = new();

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
        }

        public void CloseRsi(RsiItemViewModel vm)
        {
            if (_openRsis.Remove(vm) && CurrentOpenRsi == vm)
            {
                CurrentOpenRsi = null;
            }
        }

        public async Task New()
        {
            if (await NewRsiAction.Handle(Unit.Default))
            {
                CurrentOpenRsi = new RsiItemViewModel
                {
                    License = Preferences.DefaultLicense,
                    Copyright = Preferences.DefaultCopyright
                };

                AddRsi(CurrentOpenRsi);
            }
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

            var metaJson = metaJsonFiles.Single();
            var stream = File.OpenRead(metaJson);
            var rsi = await Rsi.FromMetaJson(stream);

            if (rsi is not {Size: {}})
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel("Error loading meta.json:\nMissing size property."));
                return;
            }

            await rsi.TryLoadFolderImages(folderPath);

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

        public async Task Open()
        {
            var folder = await OpenRsiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            await OpenRsi(folder);
        }

        private async Task SaveRsiToPath(RsiItemViewModel rsi)
        {
            if (rsi.SaveFolder == null)
            {
                return;
            }

            var minify = Locator.Current.GetService<Preferences>()!.MinifyJson;
            var options = new JsonSerializerOptions
            {
                WriteIndented = !minify,
                IgnoreNullValues = true
            };

            await rsi.Item.Rsi.SaveToFolder(rsi.SaveFolder, options);
        }

        private async Task SaveRsi(RsiItemViewModel rsi)
        {
            if (rsi.SaveFolder == null)
            {
                SaveAs();
                return;
            }

            await SaveRsiToPath(rsi);
        }

        public async void Save()
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

            await SaveRsiToPath(CurrentOpenRsi);
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
            await SaveRsiToPath(rsi);
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
                rsi.SaveFolder = $"{path}{Path.DirectorySeparatorChar}{rsi.Title}";
                await SaveRsi(rsi);
            }
        }

        public async Task ImportDmi(string filePath)
        {
            if (!DmiParser.TryGetFileMetadata(filePath, out var metadata, out var parseError))
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel(parseError.Message));
                return;
            }

            Image<Rgba32> dmi;
            try
            {
                dmi = Image.Load<Rgba32>(filePath, PngDecoder);
            }
            catch (Exception e)
            {
                Logger.Sink.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
                await ErrorDialog.Handle(new ErrorWindowViewModel($"Error loading dmi image:\n{e.Message}"));
                return;
            }

            var rsi = metadata.ToRsi(dmi);
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

        public async void Import()
        {
            var file = await ImportDmiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            await ImportDmi(file);
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
                    await ImportDmi(LastOpenedElement);
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

        public void Undo()
        {
            if (CurrentOpenRsi != null &&
                CurrentOpenRsi.TryRestore(out var selected))
            {
                CurrentOpenRsi.SelectedState = selected;
            }
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
            if (CurrentOpenRsi?.SelectedState != null)
            {
                CurrentOpenRsi.SelectedState.Image.State.Directions = directions;
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

        public void Delete()
        {
            CurrentOpenRsi?.DeleteSelectedState();
        }
    }
}
