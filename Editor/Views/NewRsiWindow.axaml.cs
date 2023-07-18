using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using ReactiveUI;

namespace Editor.Views;

public partial class ConfirmationWindow : ReactiveWindow<ConfirmationWindowViewModel>
{
    public ConfirmationWindow()
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

    private void Confirm(InteractionContext<Unit, Unit> interaction)
    {
        Close(true);
        interaction.SetOutput(Unit.Default);
    }

    private void Decline(InteractionContext<Unit, Unit> interaction)
    {
        Close(false);
        interaction.SetOutput(Unit.Default);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}