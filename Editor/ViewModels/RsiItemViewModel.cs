using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Logging;
using Editor.Models.RSI;
using Importer.RSI;
using ReactiveUI;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Editor.ViewModels
{
    public class RsiItemViewModel : ViewModelBase, IDisposable
    {
        private const int DeletedBufferSize = 50;
        private const int RestoredBufferSize = 50;

        private readonly MemoryStream _emptyStream = new();
        private readonly Bitmap _blankFrame;
        private RsiStateViewModel? _selectedState;
        private bool _hasStateSelected;

        public RsiItemViewModel(RsiItem? item = null)
        {
            Item = item ?? new RsiItem();

            foreach (var image in Item.Images)
            {
                States.Add(new RsiStateViewModel(image));
            }

            var bitmap = new System.Drawing.Bitmap(Item.Size.X, Item.Size.Y);

            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);
            g.Flush();

            bitmap.Save(EmptyStream, ImageFormat.Png);

            _blankFrame = new Bitmap(EmptyStream);
            Frames = new RsiFramesViewModel(_blankFrame, _blankFrame, _blankFrame, _blankFrame);
        }

        private MemoryStream EmptyStream
        {
            get
            {
                _emptyStream.Seek(0, SeekOrigin.Begin);
                return _emptyStream;
            }
        }

        public string Title { get; } = "Rsi"; // TODO

        public Interaction<Unit, string> ImportPngInteraction { get; } = new();

        public Interaction<ErrorWindowViewModel, Unit> ErrorDialog { get; } = new();

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
                    Frames.South = _blankFrame;
                    Frames.North = _blankFrame;
                    Frames.East = _blankFrame;
                    Frames.West = _blankFrame;
                }
                else
                {
                    RefreshFrames();
                    HasStateSelected = true;
                }
            }
        }

        public bool HasStateSelected
        {
            get => _hasStateSelected;
            set => this.RaiseAndSetIfChanged(ref _hasStateSelected, value);
        }

        public RsiFramesViewModel Frames { get; }

        private CircularBuffer<(RsiStateViewModel model, int index)> Deleted { get; } = new(DeletedBufferSize);

        private CircularBuffer<RsiStateViewModel> Restored { get; } = new(RestoredBufferSize);

        public async Task CreateNewState(string? pngFilePath = null)
        {
            var state = new RsiState(string.Empty);

            Bitmap bitmap;
            if (string.IsNullOrEmpty(pngFilePath))
            {
                bitmap = new Bitmap(EmptyStream);
            }
            else
            {
                state.Name = Path.GetFileNameWithoutExtension(pngFilePath);

                try
                {
                    bitmap = new Bitmap(pngFilePath);
                }
                catch (Exception)
                {
                    var errorVm = new ErrorWindowViewModel($"Error creating a state from file\n{pngFilePath}");
                    await ErrorDialog.Handle(errorVm);
                    return;
                }
            }

            var image = new RsiImage(state, bitmap);
            var vm = new RsiStateViewModel(image);

            AddState(vm);
        }

        public async Task ImportPng()
        {
            if (SelectedState == null)
            {
                return;
            }

            var path = await ImportPngInteraction.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!path.EndsWith(".png"))
            {
                var vm = new ErrorWindowViewModel($"File {path} is not a .png");
                await ErrorDialog.Handle(vm);
                return;
            }

            Bitmap png;
            try
            {
                png = new Bitmap(path);
            }
            catch (Exception e)
            {
                Logger.Sink.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
                var vm = new ErrorWindowViewModel($"Error opening file {path}");
                await ErrorDialog.Handle(vm);
                return;
            }

            SelectedState.Image.Bitmap.Dispose();
            SelectedState.Image.Bitmap = png;
            RefreshFrames();
        }

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

        public void AddState(RsiStateViewModel vm)
        {
            Item.AddState(vm.Image);
            States.Add(vm);
        }

        public bool TryDelete(RsiStateViewModel stateVm, [NotNullWhen(true)] out int index)
        {
            Item.RemoveState(stateVm.Image.State);

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

        public void Dispose()
        {
            _emptyStream.Dispose();
        }

        private void RefreshFrames()
        {
            if (SelectedState == null)
            {
                return;
            }

            Frames.South = SelectedState.Image.Bitmap;
            Frames.North = SelectedState.Image.Bitmap;
            Frames.East = SelectedState.Image.Bitmap;
            Frames.West = SelectedState.Image.Bitmap;
        }
    }
}
