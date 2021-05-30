using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Editor.Models.RSI;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class RsiItemViewModel : ViewModelBase
    {
        private const int DeletedBufferSize = 50;
        private const int RestoredBufferSize = 50;

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

        private CircularBuffer<(RsiStateViewModel model, int index)> Deleted { get; } = new(DeletedBufferSize);

        private CircularBuffer<RsiStateViewModel> Restored { get; } = new(RestoredBufferSize);

        public bool TryDelete(RsiStateViewModel stateVm, [NotNullWhen(true)] out int index)
        {
            Item.States.Remove(stateVm.State);

            index = States.IndexOf(stateVm);
            var removed = States.Remove(stateVm);

            if (!removed)
            {
                return false;
            }

            Deleted.PushFront((stateVm, index));
            return true;
        }

        public bool TryRestore()
        {
            if (!Deleted.TryTakeFront(out var element))
            {
                return false;
            }

            var (model, index) = element;

            Item.States.Insert(index, model.State);
            States.Insert(index, model);

            Restored.PushFront(model);
            return true;
        }

        public bool TryRedoDelete()
        {
            return Restored.TryTakeFront(out var element) &&
                   TryDelete(element, out _);
        }
    }
}
