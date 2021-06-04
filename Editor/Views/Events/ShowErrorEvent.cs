using Avalonia.Interactivity;
using Editor.ViewModels;

namespace Editor.Views.Events
{
    public class ShowErrorEvent : RoutedEventArgs
    {
        public ShowErrorEvent(ErrorWindowViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public ErrorWindowViewModel ViewModel { get; }
    }
}
