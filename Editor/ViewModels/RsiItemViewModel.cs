using System.Collections.ObjectModel;
using Editor.Models.RSI;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class RsiItemViewModel : ViewModelBase
    {
        private RsiStateViewModel? _selectedState;
        private RsiFramesViewModel? _frames;

        public RsiItemViewModel(RsiItem item)
        {
            Item = item;

            foreach (var state in Item.States)
            {
                States.Add(new RsiStateViewModel(state));
            }
        }

        public RsiItem Item { get; }

        public ObservableCollection<RsiStateViewModel> States { get; } = new();

        public RsiStateViewModel? SelectedState
        {
            get => _selectedState;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedState, value);
                Frames = value == null ? null : new RsiFramesViewModel(value.State);
            }
        }

        public RsiFramesViewModel? Frames
        {
            get => _frames;
            set => this.RaiseAndSetIfChanged(ref _frames, value);
        }
    }
}
