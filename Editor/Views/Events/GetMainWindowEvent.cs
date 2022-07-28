using Avalonia.Interactivity;

namespace Editor.Views.Events;

public class GetMainWindowEvent : RoutedEventArgs
{
    public MainWindow MainWindow { get; set; } = default!;
}