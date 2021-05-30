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

        public Interaction<Unit, string> OpenRsiDialog { get; } = new();

        public Interaction<ErrorWindowViewModel, Unit> ErrorDialog { get; } = new();

        public Interaction<RsiStateViewModel, Unit> UndoAction { get; } = new();

        public Interaction<int, Unit> RedoAction { get; } = new();

        public RsiItemViewModel? Rsi
        {
            get => _rsi;
            private set => this.RaiseAndSetIfChanged(ref _rsi, value);
        }

        public async void Open()
        {
            var folder = await OpenRsiDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

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
                var bitmap = new Bitmap($"{folder}{Path.DirectorySeparatorChar}{state.Name}.png");
                state.Image = bitmap;
            }

            Rsi = new RsiItemViewModel(rsi);
        }

        public async void Undo()
        {
            if (Rsi != null && Rsi.TryRestore(out var selected))
            {
                await UndoAction.Handle(selected);
            }
        }

        public async  void Redo()
        {
            if (Rsi != null && Rsi.TryRedoDelete(out var index))
            {
                await RedoAction.Handle(index);
            }
        }
    }
}
