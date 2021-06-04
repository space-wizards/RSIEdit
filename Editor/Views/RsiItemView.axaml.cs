using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using ReactiveUI;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class RsiItemView : ReactiveUserControl<RsiItemViewModel>
    {
        public RsiItemView()
        {
            InitializeComponent();

            this.WhenAnyValue(x => x.ViewModel)
                .Select(vm => this.WhenActivated(d =>
                {
                    if (vm == null)
                    {
                        return;
                    }

                    d.Add(vm.States.Subscribe(new AnonymousObserver<RsiStateViewModel>(s => d.Add(s.Bitmap))));
                    d.Add(vm);
                }));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
