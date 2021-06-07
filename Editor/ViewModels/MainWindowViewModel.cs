using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Logging;
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

        public MainWindowViewModel()
        {
            Preferences = Locator.Current.GetService<Preferences>();
        }

        public Preferences Preferences { get; }

        private MetadataParser DmiParser { get; } = new();

        private string? LastOpenedElement { get; set; }

        private ObservableCollection<RsiItemViewModel> OpenRsis { get; } = new();

        public RsiItemViewModel? CurrentOpenRsi
        {
            get => _currentOpenRsi;
            set => this.RaiseAndSetIfChanged(ref _currentOpenRsi, value);
        }

        public Interaction<Unit, Unit> NewRsiAction { get; } = new();

        public Interaction<Unit, string> OpenRsiDialog { get; } = new();

        public Interaction<Unit, string> SaveRsiDialog { get; } = new();

        public Interaction<Unit, string> ImportDmiDialog { get; } = new();

        public Interaction<Unit, Unit> PreferencesAction { get; } = new();

        public Interaction<ErrorWindowViewModel, Unit> ErrorDialog { get; } = new();

        public Interaction<RsiStateViewModel, Unit> UndoAction { get; } = new();

        public Interaction<int, Unit> RedoAction { get; } = new();

        public Interaction<DirectionType, Unit> DirectionsAction { get; } = new();

        public Interaction<Unit, string?> ChangeAllLicensesAction { get; } = new();

        public Interaction<Unit, string?> ChangeAllCopyrightsAction { get; } = new();

        private void AddRsi(RsiItemViewModel vm)
        {
            OpenRsis.Add(vm);
            CurrentOpenRsi = vm;
        }

        public void CloseRsi(RsiItemViewModel vm)
        {
            if (OpenRsis.Remove(vm) && CurrentOpenRsi == vm)
            {
                CurrentOpenRsi = null;
            }
        }

        public async Task New()
        {
            await NewRsiAction.Handle(Unit.Default);
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
                await ErrorDialog.Handle(new ErrorWindowViewModel($"More than one meta.json found in folder\n{folderPath}"));
                return;
            }

            var metaJson = metaJsonFiles.Single();
            var stream = File.OpenRead(metaJson);
            var rsi = await JsonSerializer.DeserializeAsync<Rsi>(stream);

            if (rsi is not {Size: {}})
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel("Error loading meta.json"));
                return;
            }

            await rsi.LoadFolderImages(folderPath);

            var rsiItem = new RsiItem(rsi);
            var name = Path.GetFileName(folderPath);
            var rsiVm = new RsiItemViewModel(name, rsiItem)
            {
                SaveFolder = folderPath,
                License = Preferences.DefaultLicense,
                Copyright = Preferences.DefaultCopyright
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

            Directory.CreateDirectory(rsi.SaveFolder);
            var metaJsonPath = $"{rsi.SaveFolder}{Path.DirectorySeparatorChar}meta.json";
            await File.WriteAllTextAsync(metaJsonPath, string.Empty);

            var metaJsonFile = File.OpenWrite(metaJsonPath);
            var minify = Locator.Current.GetService<Preferences>().MinifyJson;
            var options = new JsonSerializerOptions
            {
                WriteIndented = !minify,
                IgnoreNullValues = true
            };

            await JsonSerializer.SerializeAsync(metaJsonFile, rsi.Item.Rsi, options);
            await metaJsonFile.FlushAsync();
            await metaJsonFile.DisposeAsync();

            await rsi.Item.Rsi.SaveTo(rsi.SaveFolder);
        }

        private async Task SaveRsi(RsiItemViewModel rsi)
        {
            if (rsi.SaveFolder == null)
            {
                await SaveAs();
                return;
            }

            await SaveRsiToPath(rsi);
        }

        public async Task Save()
        {
            if (CurrentOpenRsi == null)
            {
                return;
            }

            if (CurrentOpenRsi.SaveFolder == null)
            {
                await SaveAs();
                return;
            }

            await SaveRsiToPath(CurrentOpenRsi);
        }

        private async Task SaveRsiAs(RsiItemViewModel rsi)
        {
            var path = await SaveRsiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            rsi.SaveFolder = path;
            await SaveRsiToPath(rsi);
        }

        public async Task SaveAs()
        {
            if (CurrentOpenRsi == null)
            {
                return;
            }

            await SaveRsiAs(CurrentOpenRsi);
        }

        public async Task SaveAll()
        {
            var path = await SaveRsiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            foreach (var rsi in OpenRsis)
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
                await ErrorDialog.Handle(new ErrorWindowViewModel("Error loading dmi image"));
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

        public async Task Import()
        {
            var file = await ImportDmiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            await ImportDmi(file);
        }

        public async Task ReOpenLast()
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

        public async Task OpenPreferences()
        {
            await PreferencesAction.Handle(Unit.Default);
        }

        public async Task Undo()
        {
            if (CurrentOpenRsi != null && CurrentOpenRsi.TryRestore(out var selected))
            {
                await UndoAction.Handle(selected);
            }
        }

        public async Task Redo()
        {
            if (CurrentOpenRsi != null && CurrentOpenRsi.TryRedoDelete(out var index))
            {
                await RedoAction.Handle(index);
            }
        }

        public async Task Directions(int amount)
        {
            if (CurrentOpenRsi != null)
            {
                await DirectionsAction.Handle((DirectionType) amount);
            }
        }

        public async Task ChangeAllLicenses()
        {
            var license = await ChangeAllLicensesAction.Handle(Unit.Default);
            if (license == null)
            {
                return;
            }

            foreach (var rsi in OpenRsis)
            {
                rsi.License = license;
            }
        }

        public async Task ChangeAllCopyrights()
        {
            var copyright = await ChangeAllCopyrightsAction.Handle(Unit.Default);
            if (copyright == null)
            {
                return;
            }

            foreach (var rsi in OpenRsis)
            {
                rsi.Copyright = copyright;
            }
        }
    }
}
