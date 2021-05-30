using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class RsiFramesView : UserControl
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
