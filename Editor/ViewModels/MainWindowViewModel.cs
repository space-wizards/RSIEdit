using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Editor.Models.RSI;
using Importer.DMI;
using Importer.DMI.Metadata;
using Importer.RSI;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private RsiItemViewModel? _currentOpenRsi;

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

        public Interaction<ErrorWindowViewModel, Unit> ErrorDialog { get; } = new();

        public Interaction<RsiStateViewModel, Unit> UndoAction { get; } = new();

        public Interaction<int, Unit> RedoAction { get; } = new();

        public Interaction<DirectionTypes, Unit> DirectionsAction { get; } = new();

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

        public async void New()
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

            var rsiItem = new RsiItem(rsi);

            if (!rsiItem.TryLoadImages(folderPath, out var error))
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel(error));
            }

            var name = Path.GetFileName(folderPath);
            var rsiVm = new RsiItemViewModel(name, rsiItem) {SaveFolder = folderPath};

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

        private async Task SaveRsi(string path)
        {
            if (CurrentOpenRsi == null)
            {
                return;
            }

            var metaJsonPath = $"{path}{Path.DirectorySeparatorChar}meta.json";
            await File.WriteAllTextAsync(metaJsonPath, string.Empty);

            var metaJsonFile = File.OpenWrite(metaJsonPath);
            await JsonSerializer.SerializeAsync(metaJsonFile, CurrentOpenRsi.Item.Rsi);
            await metaJsonFile.FlushAsync();
            await metaJsonFile.DisposeAsync();

            foreach (var image in CurrentOpenRsi.Item.Images)
            {
                image.Bitmap.Save($"{path}{Path.DirectorySeparatorChar}{image.State.Name}.png");
            }
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

            await SaveRsi(CurrentOpenRsi.SaveFolder);
        }

        public async Task SaveAs()
        {
            if (CurrentOpenRsi == null)
            {
                return;
            }

            var path = await SaveRsiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            await SaveRsi(path);
            CurrentOpenRsi.SaveFolder = path;
        }

        public async Task ImportDmi(string filePath)
        {
            if (!DmiParser.TryGetFileMetadata(filePath, out var metadata, out var parseError))
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel(parseError.Message));
                return;
            }

            var rsi = metadata.ToRsi();
            var rsiItem = new RsiItem(metadata.ToRsi());

            await foreach (var (stream, index) in rsi.LoadStreams(filePath))
            {
                rsiItem.LoadImage(index, new Bitmap(stream));
            }

            var name = Path.GetFileNameWithoutExtension(filePath);
            var rsiVm = new RsiItemViewModel(name, rsiItem) {SaveFolder = null};

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

        public async void Undo()
        {
            if (CurrentOpenRsi != null && CurrentOpenRsi.TryRestore(out var selected))
            {
                await UndoAction.Handle(selected);
            }
        }

        public async void Redo()
        {
            if (CurrentOpenRsi != null && CurrentOpenRsi.TryRedoDelete(out var index))
            {
                await RedoAction.Handle(index);
            }
        }

        public async void Directions(int amount)
        {
            if (CurrentOpenRsi != null)
            {
                await DirectionsAction.Handle((DirectionTypes) amount);
            }
        }
    }
}
