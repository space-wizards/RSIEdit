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
        private bool _hasStateSelected;
        private RsiFramesViewModel? _frames;

        public RsiItemViewModel(RsiItem? item = null)
        {
            Item = item ?? new RsiItem();

            foreach (var image in Item.Images)
            {
                States.Add(new RsiStateViewModel(image));
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

                if (value == null)
                {
                    HasStateSelected = false;
                    Frames = null;
                }
                else
                {
                    Frames = new RsiFramesViewModel(value.Bitmap, value.Bitmap, value.Bitmap, value.Bitmap); // TODO
                    HasStateSelected = true;
                }
            }
        }

        public bool HasStateSelected
        {
            get => _hasStateSelected;
            set => this.RaiseAndSetIfChanged(ref _hasStateSelected, value);
        }

        public RsiFramesViewModel? Frames
        {
            get => _frames;
            set => this.RaiseAndSetIfChanged(ref _frames, value);
        }

        private CircularBuffer<(RsiStateViewModel model, int index)> Deleted { get; } = new(DeletedBufferSize);

        private CircularBuffer<RsiStateViewModel> Restored { get; } = new(RestoredBufferSize);

        public void DeleteSelectedState()
        {
            if (_selectedState == null)
            {
                return;
            }

            if (TryDelete(_selectedState, out var index))
            {
                ReselectState(index);
            }
        }

        public bool TryDelete(RsiStateViewModel stateVm, [NotNullWhen(true)] out int index)
        {
            Item.RemoveState(stateVm.State);

            index = States.IndexOf(stateVm);
            var removed = States.Remove(stateVm);

            if (!removed)
            {
                return false;
            }

            if (SelectedState == stateVm)
            {
                SelectedState = null;
            }

            Deleted.PushFront((stateVm, index));
            return true;
        }

        public bool TryRestore([NotNullWhen(true)] out RsiStateViewModel? restored)
        {
            if (!Deleted.TryTakeFront(out var element))
            {
                restored = null;
                return false;
            }

            var (model, index) = element;

            Item.InsertState(index, model.Image);
            States.Insert(index, model);

            Restored.PushFront(model);
            restored = model;
            return true;
        }

        public bool TryRedoDelete([NotNullWhen(true)] out int index)
        {
            index = -1;

            return Restored.TryTakeFront(out var element) &&
                   TryDelete(element, out index);
        }

        public void ReselectState(int deletedIndex)
        {
            int? nextSelectedState = null;

            // Select either the next or previous one
            if (States.Count > deletedIndex)
            {
                nextSelectedState = deletedIndex;
            }
            else if (States.Count == deletedIndex && States.Count > 0)
            {
                nextSelectedState = deletedIndex - 1;
            }

            if (nextSelectedState != null)
            {
                SelectedState = States[nextSelectedState.Value];
            }
        }
    }
}
