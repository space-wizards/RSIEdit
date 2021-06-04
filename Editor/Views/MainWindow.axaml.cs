using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using Editor.Views.Commands;
using Editor.Views.Events;
using Importer.DMI;
using ReactiveUI;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d =>
            {
                var vm = ViewModel!;

                d.Add(vm.NewRsiAction.RegisterHandler(NewRsi));
                d.Add(vm.OpenRsiDialog.RegisterHandler(OpenRsi));
                d.Add(vm.SaveRsiDialog.RegisterHandler(SaveRsi));
                d.Add(vm.ImportDmiDialog.RegisterHandler(ImportDmi));
                d.Add(vm.UndoAction.RegisterHandler(Undo));
                d.Add(vm.RedoAction.RegisterHandler(Redo));
                d.Add(vm.DirectionsAction.RegisterHandler(ChangeDirections));
                d.Add(vm.ErrorDialog.RegisterHandler(ShowError));
            });

            ShowErrorEvent.AddClassHandler<MainWindow>(OnShowError);
            OpenFileEvent.AddClassHandler<MainWindow>(OnOpenFile);
            AddHandler(DragDrop.DropEvent, DropEvent);
        }

        public static RoutedEvent<ShowErrorEvent> ShowErrorEvent { get; } =
            RoutedEvent.Register<MainWindow, ShowErrorEvent>(nameof(ShowErrorEvent), RoutingStrategies.Bubble);

        public static RoutedEvent<OpenFileDialogEvent> OpenFileEvent { get; } =
            RoutedEvent.Register<MainWindow, OpenFileDialogEvent>(nameof(OpenFileEvent), RoutingStrategies.Bubble);

        private NewCommand NewCommand { get; } = new();

        private DeleteStateCommand DeleteStateCommand { get; } = new();

        private UndoCommand UndoCommand { get; } = new();

        private RedoCommand RedoCommand { get; } = new();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var newGesture = new KeyGesture(Key.N, KeyModifiers.Control);
            var newBinding = new KeyBinding
            {
                Command = NewCommand,
                CommandParameter = this,
                Gesture = newGesture
            };

            var deleteGesture = new KeyGesture(Key.Delete);
            var deleteBinding = new KeyBinding
            {
                Command = DeleteStateCommand,
                CommandParameter = this,
                Gesture = deleteGesture
            };

            var undoGesture = new KeyGesture(Key.Z, KeyModifiers.Control);
            var undoBinding = new KeyBinding
            {
                Command = UndoCommand,
                CommandParameter = this,
                Gesture = undoGesture
            };

            var redoGesture = new KeyGesture(Key.Y, KeyModifiers.Control);
            var redoBinding = new KeyBinding
            {
                Command = RedoCommand,
                CommandParameter = this,
                Gesture = redoGesture
            };

            var redoGestureAlternative = new KeyGesture(Key.Z, KeyModifiers.Control | KeyModifiers.Shift);
            var redoBindingAlternative = new KeyBinding
            {
                Command = RedoCommand,
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
            if (ViewModel?.CurrentOpenRsi == null)
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
                ViewModel.CurrentOpenRsi = new RsiItemViewModel();
            }
        }

        private void NewRsi(InteractionContext<Unit, Unit> interaction)
        {
            DoNewRsi();
            interaction.SetOutput(Unit.Default);
        }

        private async Task OpenRsi(InteractionContext<Unit, string> interaction)
        {
            var dialog = new OpenFolderDialog {Title = "Open RSI"};
            var folder = await dialog.ShowAsync(this);

            interaction.SetOutput(folder);
        }

        private async Task SaveRsi(InteractionContext<Unit, string> interaction)
        {
            var dialog = new OpenFolderDialog {Title = "Save RSI"};
            var folder = await dialog.ShowAsync(this);

            interaction.SetOutput(folder);
        }

        private async Task ImportDmi(InteractionContext<Unit, string> interaction)
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
                        Extensions = new List<string> {"dmi"}
                    }
                }
            };
            var files = await dialog.ShowAsync(this);

            interaction.SetOutput(files.Length > 0 ? files[0] : string.Empty);
        }

        private void Undo(InteractionContext<RsiStateViewModel, Unit> interaction)
        {
            if (ViewModel?.CurrentOpenRsi == null)
            {
                return;
            }

            var restoredModel = interaction.Input;
            ViewModel.CurrentOpenRsi.SelectedState = restoredModel;
            interaction.SetOutput(Unit.Default);
        }

        private void Redo(InteractionContext<int, Unit> interaction)
        {
            if (ViewModel?.CurrentOpenRsi == null)
            {
                return;
            }

            ViewModel.CurrentOpenRsi.ReselectState(interaction.Input);
            interaction.SetOutput(Unit.Default);
        }

        private void ChangeDirections(InteractionContext<DirectionTypes, Unit> interaction)
        {
            if (ViewModel?.CurrentOpenRsi?.SelectedState == null)
            {
                return;
            }

            ViewModel.CurrentOpenRsi.SelectedState.Image.State.Directions = interaction.Input;
            interaction.SetOutput(Unit.Default);
        }

        private async Task ShowError(InteractionContext<ErrorWindowViewModel, Unit> interaction)
        {
            var dialog = new ErrorWindow {DataContext = interaction.Input};
            await dialog.ShowDialog(this);
            interaction.SetOutput(Unit.Default);
        }

        private async void DropEvent(object? sender, DragEventArgs e)
        {
            var fileNames = e.Data.GetFileNames();
            if (ViewModel == null || fileNames == null)
            {
                return;
            }

            var files = fileNames.ToArray();
            if (files.Length == 0)
            {
                return;
            }

            var firstFile = files[0];
            if (File.GetAttributes(firstFile).HasFlag(FileAttributes.Directory))
            {
                if (await TryOpenRsiConfirmationPopup("Are you sure you want to open a new RSI?"))
                {
                    await ViewModel.OpenRsi(firstFile);
                }

                return;
            }

            switch (Path.GetExtension(firstFile))
            {
                case ".dmi":
                    if (await TryOpenRsiConfirmationPopup("Are you sure you want to import a new DMI?"))
                    {
                        await ViewModel.ImportDmi(firstFile);
                    }

                    break;
                case ".png":
                    foreach (var file in files)
                    {
                        ViewModel.CurrentOpenRsi?.CreateNewState(file);
                    }

                    break;
            }
        }

        private async void OnShowError(MainWindow window, ShowErrorEvent args)
        {
            var dialog = new ErrorWindow {DataContext = args.ViewModel};
            await dialog.ShowDialog(this);
        }

        private void OnOpenFile(MainWindow window, OpenFileDialogEvent args)
        {
            var files = args.Dialog.ShowAsync(window).Result;
            args.Files = files;
        }
    }
}
