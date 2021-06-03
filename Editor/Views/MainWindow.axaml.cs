using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using Editor.Views.RsiItemCommands;
using Importer.DMI;
using ReactiveUI;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly NewCommand _newCommand = new();
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
                d.Add(ViewModel!.NewRsiAction.RegisterHandler(DoNewRsi));
                d.Add(ViewModel!.OpenRsiDialog.RegisterHandler(DoOpenRsi));
                d.Add(ViewModel!.SaveRsiDialog.RegisterHandler(DoSaveRsi));
                d.Add(ViewModel!.ImportDmiDialog.RegisterHandler(DoImportDmi));
                d.Add(ViewModel!.UndoAction.RegisterHandler(DoUndo));
                d.Add(ViewModel!.RedoAction.RegisterHandler(DoRedo));
                d.Add(ViewModel!.DirectionsAction.RegisterHandler(DoChangeDirections));
                d.Add(ViewModel!.ErrorDialog.RegisterHandler(DoShowError));
            });

            RsiItemView = this.Find<RsiItemView>("RsiItemView");
            AddHandler(DragDrop.DropEvent, DropEvent);
        }

        private RsiItemView RsiItemView { get; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var newGesture = new KeyGesture(Key.N, KeyModifiers.Control);
            var newBinding = new KeyBinding
            {
                Command = _newCommand,
                CommandParameter = this,
                Gesture = newGesture
            };

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
                newBinding,
                deleteBinding,
                undoBinding,
                redoBinding,
                redoBindingAlternative
            });
        }

        private async Task<bool> TryOpenRsiConfirmationPopup(string text)
        {
            if (ViewModel?.Rsi == null)
            {
                return true;
            }

            var newVm = new NewRsiWindowViewModel(text);
            var confirmed = await new NewRsiWindow() {ViewModel = newVm}.ShowDialog<bool>(this);

            return confirmed;
        }

        public async void DoNewRsi()
        {
            if (ViewModel == null)
            {
                return;
            }

            if (await TryOpenRsiConfirmationPopup("Are you sure you want to create a new RSI?"))
            {
                ViewModel.Rsi = new RsiItemViewModel();
            }
        }

        private void DoNewRsi(InteractionContext<Unit, Unit> interaction)
        {
            DoNewRsi();
            interaction.SetOutput(Unit.Default);
        }

        private async Task DoOpenRsi(InteractionContext<Unit, string> interaction)
        {
            var dialog = new OpenFolderDialog {Title = "Open RSI"};
            var folder = await dialog.ShowAsync(this);

            interaction.SetOutput(folder);
        }

        private async Task DoSaveRsi(InteractionContext<Unit, string> interaction)
        {
            var dialog = new OpenFolderDialog {Title = "Save RSI"};
            var folder = await dialog.ShowAsync(this);

            interaction.SetOutput(folder);
        }

        private async Task DoImportDmi(InteractionContext<Unit, string> interaction)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import DMI",
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Name = "DMI Files",
                        Extensions = new List<string>
                        {
                            "dmi"
                        }
                    }
                }
            };
            var files = await dialog.ShowAsync(this);

            interaction.SetOutput(files.Length > 0 ? files[0] : string.Empty);
        }

        private void DoUndo(InteractionContext<RsiStateViewModel, Unit> interaction)
        {
            if (ViewModel?.Rsi == null)
            {
                return;
            }

            var restoredModel = interaction.Input;
            ViewModel.Rsi.SelectedState = restoredModel;
            interaction.SetOutput(Unit.Default);
        }

        private void DoRedo(InteractionContext<int, Unit> interaction)
        {
            if (ViewModel?.Rsi == null)
            {
                return;
            }

            ViewModel.Rsi.ReselectState(interaction.Input);
            interaction.SetOutput(Unit.Default);
        }

        private void DoChangeDirections(InteractionContext<DirectionTypes, Unit> interaction)
        {
            if (ViewModel?.Rsi?.SelectedState == null)
            {
                return;
            }

            ViewModel.Rsi.SelectedState.State.Directions = interaction.Input;
            interaction.SetOutput(Unit.Default);
        }

        private async Task DoShowError(InteractionContext<ErrorWindowViewModel, Unit> interaction)
        {
            var dialog = new ErrorWindow {DataContext = interaction.Input};
            await dialog.ShowDialog(this);
            interaction.SetOutput(Unit.Default);
        }

        private async void DropEvent(object? sender, DragEventArgs e)
        {
            var files = e.Data.GetFileNames();
            if (ViewModel == null || files == null)
            {
                return;
            }

            var file = files.ToArray()[0];
            if (File.GetAttributes(file).HasFlag(FileAttributes.Directory))
            {
                if (await TryOpenRsiConfirmationPopup("Are you sure you want to open a new RSI?"))
                {
                    await ViewModel.OpenRsi(file);
                }

                return;
            }

            if (!file.EndsWith(".dmi"))
            {
                return;
            }

            if (await TryOpenRsiConfirmationPopup("Are you sure you want to import a new DMI?"))
            {
                await ViewModel.ImportDmi(file);
            }
        }
    }
}
