using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Editor.Views
{
    public class RsiStateView : UserControl
    {
        public RsiStateView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
