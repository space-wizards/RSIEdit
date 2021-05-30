using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Editor.Views
{
    public class RsiFramesView : UserControl
    {
        public RsiFramesView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
