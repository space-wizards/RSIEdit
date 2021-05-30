using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class RsiStateView : UserControl
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
