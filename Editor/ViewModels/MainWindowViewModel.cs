using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using Avalonia.Media.Imaging;
using Editor.Models.RSI;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private RsiItemViewModel? _rsi;

        private string? LastOpenedFolder { get; set; }

        private string? SaveFolder { get; set; }

        public Interaction<Unit, Unit> NewRsiAction { get; } = new();

        public Interaction<Unit, string> OpenRsiDialog { get; } = new();

        public Interaction<Unit, string> SaveRsiDialog { get; } = new();

        public Interaction<Unit, string> ImportDmiDialog { get; } = new();

        public Interaction<ErrorWindowViewModel, Unit> ErrorDialog { get; } = new();

        public Interaction<RsiStateViewModel, Unit> UndoAction { get; } = new();

        public Interaction<int, Unit> RedoAction { get; } = new();

        public Interaction<RsiStateDirections, Unit> DirectionsAction { get; } = new();

        public RsiItemViewModel? Rsi
        {
            get => _rsi;
            set => this.RaiseAndSetIfChanged(ref _rsi, value);
        }

        public async void New()
        {
            await NewRsiAction.Handle(Unit.Default);
        }

        private async void OpenFolder(string folder)
        {
            var metaJsonFiles = Directory.GetFiles(folder, "meta.json");

            if (metaJsonFiles.Length == 0)
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel("No meta.json found in folder"));
                return;
            }

            if (metaJsonFiles.Length > 1)
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel("More than one meta.json found in folder"));
                return;
            }

            var metaJson = metaJsonFiles.Single();
            var stream = File.OpenRead(metaJson);
            var rsi = await JsonSerializer.DeserializeAsync<RsiItem>(stream);

            if (rsi == null)
            {
                await ErrorDialog.Handle(new ErrorWindowViewModel("Error loading meta.json"));
                return;
            }

            foreach (var state in rsi.States)
            {
                var statePath = $"{folder}{Path.DirectorySeparatorChar}{state.Name}.png";

                if (!File.Exists(statePath))
                {
                    await ErrorDialog.Handle(new ErrorWindowViewModel($"Missing state found in meta.json:\n{statePath}"));
                }

                var bitmap = new Bitmap(statePath);
                state.Image = bitmap;
            }

            Rsi = new RsiItemViewModel(rsi);
            LastOpenedFolder = folder;
            SaveFolder = null;
        }

        public async void Open()
        {
            var folder = await OpenRsiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            OpenFolder(folder);
        }

        private async void SaveRsi(string path)
        {
            var metaJsonPath = $"{path}{Path.DirectorySeparatorChar}meta.json";
            await File.WriteAllTextAsync(metaJsonPath, string.Empty);

            var metaJsonFile = File.OpenWrite(metaJsonPath);
            await JsonSerializer.SerializeAsync(metaJsonFile, Rsi.Item);
            await metaJsonFile.FlushAsync();
            await metaJsonFile.DisposeAsync();

            foreach (var state in Rsi.Item.States)
            {
                state.Image.Save($"{path}{Path.DirectorySeparatorChar}{state.Name}.png");
            }
        }

        public async void Save()
        {
            if (Rsi == null)
            {
                return;
            }

            if (SaveFolder == null)
            {
                SaveAs();
                return;
            }

            SaveRsi(SaveFolder);
        }

        public async void SaveAs()
        {
            var path = await SaveRsiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            SaveRsi(path);
            SaveFolder = path;
        }

        public async void Import()
        {
            var file = await ImportDmiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            // TODO
        }

        public void ReOpenLast()
        {
            if (LastOpenedFolder != null)
            {
                OpenFolder(LastOpenedFolder);
            }
        }

        public async void Undo()
        {
            if (Rsi != null && Rsi.TryRestore(out var selected))
            {
                await UndoAction.Handle(selected);
            }
        }

        public async void Redo()
        {
            if (Rsi != null && Rsi.TryRedoDelete(out var index))
            {
                await RedoAction.Handle(index);
            }
        }

        public async void Directions(int amount)
        {
            if (Rsi != null)
            {
                await DirectionsAction.Handle((RsiStateDirections) amount);
            }
        }
    }
}
