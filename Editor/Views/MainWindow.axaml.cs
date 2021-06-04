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
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Editor.Models;
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
                d.Add(vm.PreferencesAction.RegisterHandler(OpenPreferences));
                d.Add(vm.UndoAction.RegisterHandler(Undo));
                d.Add(vm.RedoAction.RegisterHandler(Redo));
                d.Add(vm.DirectionsAction.RegisterHandler(ChangeDirections));
                d.Add(vm.ErrorDialog.RegisterHandler(ShowError));
                d.Add(vm.ChangeAllLicensesAction.RegisterHandler(ChangeAllLicenses));
                d.Add(vm.ChangeAllCopyrightsAction.RegisterHandler(ChangeAllCopyrights));
            });

            ShowErrorEvent.AddClassHandler<MainWindow>(OnShowError);
            OpenFileEvent.AddClassHandler<MainWindow>(OnOpenFile);
            AskConfirmationEvent.AddClassHandler<MainWindow>(OnAskConfirmation);
            CloseRsiEvent.AddClassHandler<MainWindow>(OnCloseRsi);
            GetMainWindowEvent.AddClassHandler<MainWindow>(OnGetMainWindow);

            AddHandler(DragDrop.DropEvent, DropEvent);
        }

        public static RoutedEvent<ShowErrorEvent> ShowErrorEvent { get; } =
            RoutedEvent.Register<MainWindow, ShowErrorEvent>(nameof(ShowErrorEvent), RoutingStrategies.Bubble);

        public static RoutedEvent<OpenFileDialogEvent> OpenFileEvent { get; } =
            RoutedEvent.Register<MainWindow, OpenFileDialogEvent>(nameof(OpenFileEvent), RoutingStrategies.Bubble);

        public static RoutedEvent<AskConfirmationEvent> AskConfirmationEvent { get; } =
            RoutedEvent.Register<MainWindow, AskConfirmationEvent>(nameof(AskConfirmationEvent), RoutingStrategies.Bubble);

        public static RoutedEvent<CloseRsiEvent> CloseRsiEvent { get; } =
            RoutedEvent.Register<MainWindow, CloseRsiEvent>(nameof(CloseRsiEvent), RoutingStrategies.Bubble);

        public static RoutedEvent<GetMainWindowEvent> GetMainWindowEvent { get; } =
            RoutedEvent.Register<MainWindow, GetMainWindowEvent>(nameof(GetMainWindowEvent), RoutingStrategies.Bubble);

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

        private async Task<bool> TryOpenConfirmation(string text, bool modified = true)
        {
            if (!modified || ViewModel?.CurrentOpenRsi == null)
            {
                return true;
            }

            var newVm = new ConfirmationWindowViewModel(text);
            var confirmed = await new ConfirmationWindow() {ViewModel = newVm}.ShowDialog<bool>(this);

            return confirmed;
        }

        public async void DoNewRsi()
        {
            if (ViewModel == null)
            {
                return;
            }

            if (await TryOpenConfirmation("Are you sure you want to create a new RSI?"))
            {
                ViewModel.CurrentOpenRsi = new RsiItemViewModel
                {
                    License = ViewModel.Preferences.DefaultLicense,
                    Copyright = ViewModel.Preferences.DefaultCopyright
                };
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

        private async Task OpenPreferences(InteractionContext<Unit, Unit> arg)
        {
            if (ViewModel == null)
            {
                return;
            }

            var vm = new PreferencesWindowViewModel(ViewModel.Preferences);
            var dialog = new PreferencesWindow() {DataContext = vm};
            var preferences = await dialog.ShowDialog<Preferences>(this);

            if (preferences.EasterEggs)
            {
                Icon = new WindowIcon("./Assets/joke-logo.ico");
                Background = new ImageBrush(new Bitmap("./Assets/joke-background.png"));
            }
            else
            {
                Icon = new WindowIcon("./Assets/logo.ico");
                Background = null;
            }

            arg.SetOutput(Unit.Default);
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

        private async Task ChangeAllLicenses(InteractionContext<Unit, string?> arg)
        {
            var vm = new TextInputWindowViewModel("Change all licenses", "Change all open RSI licenses to:");
            var dialog = new TextInputWindow {DataContext = vm};

            if (!await dialog.ShowDialog<bool>(this))
            {
                arg.SetOutput(null);
                return;
            }

            arg.SetOutput(vm.SubmittedText);
        }

        private async Task ChangeAllCopyrights(InteractionContext<Unit, string?> arg)
        {
            var vm = new TextInputWindowViewModel("Change all copyrights", "Change all open RSI copyrights to:");
            var dialog = new TextInputWindow {DataContext = vm};

            if (!await dialog.ShowDialog<bool>(this))
            {
                arg.SetOutput(null);
                return;
            }

            arg.SetOutput(vm.SubmittedText);
        }

        private async void DropEvent(object? sender, DragEventArgs e)
        {
            var fileNames = e.Data.GetFileNames();
            if (ViewModel == null || fileNames == null)
            {
                return;
            }

            var files = fileNames.ToArray();
            var rsiDmiToOpen = new List<string>();

            foreach (var file in files)
            {
                if (File.GetAttributes(file).HasFlag(FileAttributes.Directory))
                {
                    rsiDmiToOpen.Add(file);
                    continue;
                }

                switch (Path.GetExtension(file))
                {
                    case ".dmi":
                        rsiDmiToOpen.Add(file);
                        break;
                    case ".png":
                        ViewModel.CurrentOpenRsi?.CreateNewState(file);
                        break;
                }
            }

            foreach (var rsiOrDmi in rsiDmiToOpen)
            {
                switch (Path.GetExtension(rsiOrDmi))
                {
                    case ".dmi":
                        await ViewModel.ImportDmi(rsiOrDmi);
                        break;
                    default:
                        await ViewModel.OpenRsi(rsiOrDmi);
                        break;
                }
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

        private void OnAskConfirmation(MainWindow window, AskConfirmationEvent args)
        {
            var dialog = new ConfirmationWindow {DataContext = args.ViewModel};
            args.Confirmed = dialog.ShowDialog<bool>(this).Result;
        }

        private async void OnCloseRsi(MainWindow window, CloseRsiEvent args)
        {
            if (ViewModel != null && await TryOpenConfirmation("Are you sure you want to close the current RSI without saving?", args.ViewModel.Modified))
            {
                ViewModel?.CloseRsi(args.ViewModel);
            }
        }

        private void OnGetMainWindow(MainWindow window, GetMainWindowEvent args)
        {
            args.MainWindow = window;
        }
    }
}
