using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Editor.Views
{
    public partial class RsiItemView : UserControl
    {
        public RsiItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
