using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Editor.Views.Events
{
    public class OpenFileDialogEvent : RoutedEventArgs
    {
        public OpenFileDialogEvent(OpenFileDialog dialog)
        {
            Dialog = dialog;
        }

        public OpenFileDialog Dialog { get; }

        public string[] Files { get; set; } = new string[0];
    }
}
