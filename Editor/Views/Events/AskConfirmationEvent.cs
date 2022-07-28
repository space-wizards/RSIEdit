using Avalonia.Interactivity;
using Editor.ViewModels;

namespace Editor.Views.Events;

public class AskConfirmationEvent : RoutedEventArgs
{
    public AskConfirmationEvent(ConfirmationWindowViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public ConfirmationWindowViewModel ViewModel { get; }

    public bool Confirmed { get; set; }
}