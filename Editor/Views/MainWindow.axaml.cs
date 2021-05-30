using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using Editor.Views.RsiItemCommands;
using ReactiveUI;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly DeleteStateCommand _deleteStateCommand = new();
        private readonly UndoCommand _undoCommand = new();
        private readonly RedoCommand _redoCommand = new();

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d =>
            {
                d.Add(ViewModel!.OpenRsiDialog.RegisterHandler(DoShowOpenRsiDialog));
                d.Add(ViewModel!.ErrorDialog.RegisterHandler(DoShowError));
                d.Add(ViewModel!.UndoAction.RegisterHandler(DoUndo));
                d.Add(ViewModel!.RedoAction.RegisterHandler(DoRedo));
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var deleteGesture = new KeyGesture(Key.Delete);
            var deleteBinding = new KeyBinding
            {
                Command = _deleteStateCommand,
                CommandParameter = this,
                Gesture = deleteGesture
            };

            var undoGesture = new KeyGesture(Key.Z, KeyModifiers.Control);
            var undoBinding = new KeyBinding
            {
                Command = _undoCommand,
                CommandParameter = this,
                Gesture = undoGesture
            };

            var redoGesture = new KeyGesture(Key.Y, KeyModifiers.Control);
            var redoBinding = new KeyBinding
            {
                Command = _redoCommand,
                CommandParameter = this,
                Gesture = redoGesture
            };

            var redoGestureAlternative = new KeyGesture(Key.Z, KeyModifiers.Control | KeyModifiers.Shift);
            var redoBindingAlternative = new KeyBinding
            {
                Command = _redoCommand,
                CommandParameter = this,
                Gesture = redoGestureAlternative
            };

            KeyBindings.AddRange(new[]
            {
                deleteBinding,
                undoBinding,
                redoBinding,
                redoBindingAlternative
            });
        }

        private async Task DoShowOpenRsiDialog(InteractionContext<Unit, string> interaction)
        {
            var dialog = new OpenFolderDialog {Title = "Open RSI"};
            var folder = await dialog.ShowAsync(this);

            interaction.SetOutput(folder);
        }

        private async Task DoShowError(InteractionContext<ErrorWindowViewModel, Unit> interaction)
        {
            var dialog = new ErrorWindow {DataContext = interaction.Input};
            await dialog.ShowDialog(this);
        }

        private void DoUndo(InteractionContext<RsiStateViewModel, Unit> arg)
        {
            if (ViewModel?.Rsi == null)
            {
                return;
            }

            var restoredModel = arg.Input;
            ViewModel.Rsi.SelectedState = restoredModel;
        }

        private void DoRedo(InteractionContext<int, Unit> arg)
        {
            if (ViewModel?.Rsi == null)
            {
                return;
            }

            ViewModel.Rsi.ReselectState(arg.Input);
        }
    }
}
