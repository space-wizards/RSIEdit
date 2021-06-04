using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using ReactiveUI;

namespace Editor.Views
{
    public class ConfirmationWindow : ReactiveWindow<ConfirmationWindowViewModel>
    {
        public ConfirmationWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d =>
            {
                d.Add(ViewModel!.ConfirmAction.RegisterHandler(DoConfirm));
                d.Add(ViewModel!.DeclineAction.RegisterHandler(DoDecline));
            });
        }

        private void DoConfirm(InteractionContext<Unit, Unit> interaction)
        {
            Close(true);
            interaction.SetOutput(Unit.Default);
        }

        private void DoDecline(InteractionContext<Unit, Unit> interaction)
        {
            Close(false);
            interaction.SetOutput(Unit.Default);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
