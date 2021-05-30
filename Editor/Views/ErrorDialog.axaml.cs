using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Editor.Views
{
    public class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
