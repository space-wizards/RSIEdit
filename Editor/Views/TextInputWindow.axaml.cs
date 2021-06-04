using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using ReactiveUI;

namespace Editor.Views
{
    public class TextInputWindow : ReactiveWindow<TextInputWindowViewModel>
    {
        public TextInputWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(d =>
            {
                d.Add(ViewModel!.ConfirmAction.RegisterHandler(Confirm));
                d.Add(ViewModel!.DeclineAction.RegisterHandler(Decline));
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Confirm(InteractionContext<string, Unit> arg)
        {
            Close(true);
            arg.SetOutput(Unit.Default);
        }

        private void Decline(InteractionContext<Unit, Unit> arg)
        {
            Close(false);
            arg.SetOutput(Unit.Default);
        }
    }
}
