using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
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
                d.Add(ViewModel!.OpenRsiDialog.RegisterHandler(DoShowOpenRsiDialog));
                d.Add(ViewModel!.ErrorDialog.RegisterHandler(DoShowErrorAsync));
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowOpenRsiDialog(InteractionContext<Unit, string> interaction)
        {
            var dialog = new OpenFolderDialog {Title = "Open RSI"};
            var folder = await dialog.ShowAsync(this);

            interaction.SetOutput(folder);
        }

        private async Task DoShowErrorAsync(InteractionContext<ErrorWindowViewModel, Unit> interaction)
        {
            var dialog = new ErrorWindow {DataContext = interaction.Input};
            await dialog.ShowDialog(this);
        }
    }
}
