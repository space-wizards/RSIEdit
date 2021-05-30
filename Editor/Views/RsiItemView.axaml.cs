using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class RsiItemView : ReactiveUserControl<RsiItemViewModel>
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
