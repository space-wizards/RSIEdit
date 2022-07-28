using Avalonia.Interactivity;
using Editor.ViewModels;

namespace Editor.Views.Events;

public class CloseRsiEvent : RoutedEventArgs
{
    public CloseRsiEvent(RsiItemViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public RsiItemViewModel ViewModel { get; }
}