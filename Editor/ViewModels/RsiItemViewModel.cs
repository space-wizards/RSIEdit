using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Logging;
using Editor.Models.RSI;
using Importer.Directions;
using Importer.RSI;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = SixLabors.ImageSharp.Rectangle;

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
        private string _title;
        private ComboBoxItem? _selectedLicense;

        public RsiItemViewModel(string title = "New Rsi", RsiItem? item = null)
        {
            _title = title;
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

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public bool Modified { get; private set; }

        public string? SaveFolder { get; set; }

        public Interaction<Unit, string> ImportPngInteraction { get; } = new();

        public Interaction<ErrorWindowViewModel, Unit> ErrorDialog { get; } = new();

        public Interaction<RsiItemViewModel, Unit> CloseInteraction { get; } = new();

        // TODO encapsulate this further
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

        public string? License
        {
            get => Item.Rsi.License;
            set
            {
                if (EqualityComparer<string>.Default.Equals(License, value))
                {
                    return;
                }

                this.RaisePropertyChanging();
                Item.Rsi.License = value;
                this.RaisePropertyChanged();
            }
        }

        public string? Copyright
        {
            get => Item.Rsi.Copyright;
            set
            {
                if (EqualityComparer<string>.Default.Equals(Copyright, value))
                {
                    return;
                }


                this.RaisePropertyChanging();
                Item.Rsi.Copyright = value;
                this.RaisePropertyChanged();
            }
        }

        public ComboBoxItem? SelectedLicense
        {
            get => _selectedLicense;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedLicense, value);

                if (value == null)
                {
                    License = null;
                    return;
                }

                var valueLicense = (string) value.Content;

                if (valueLicense == "None")
                {
                    License = null;
                    return;
                }

                License = valueLicense;
            }
        }

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

            var image = new RsiImage(Item.Size, state, bitmap);
            var vm = new RsiStateViewModel(image);

            AddState(vm);
            Modified = true;
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
            Modified = true;
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

        public async Task Close()
        {
            await CloseInteraction.Handle(this);
        }

        public void AddState(RsiStateViewModel vm)
        {
            Item.AddState(vm.Image);
            States.Add(vm);
            Modified = true;
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
            Modified = true;
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

        private void RefreshFrames()
        {
            if (SelectedState == null)
            {
                return;
            }

            var state = SelectedState;

            switch (state.Image.State.Directions)
            {
                case DirectionType.None:
                    Frames.South = state.Image.Bitmap;
                    Frames.North = state.Image.Bitmap;
                    Frames.East = state.Image.Bitmap;
                    Frames.West = state.Image.Bitmap;
                    break;
                case DirectionType.Cardinal:
                case DirectionType.Diagonal:
                {
                    var delays = state.Image.State.Delays;
                    if (delays == null)
                    {
                        Frames.South = state.Image.Bitmap;
                        Frames.North = state.Image.Bitmap;
                        Frames.East = state.Image.Bitmap;
                        Frames.West = state.Image.Bitmap;
                        return;
                    }

                    using var stream = new MemoryStream();
                    state.Image.Bitmap.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    var fullImage = Image.Load<Rgba32>(stream, new PngDecoder());

                    var delaysIterated = 0;

                    var directionalImages = new Bitmap[4];

                    for (var i = 0; i < 4; i++)
                    {
                        var totalDelays = delays[i].Count;
                        var totalWidth = delaysIterated * Item.Size.X;
                        var row = totalWidth / fullImage.Width;
                        var offset = totalWidth % fullImage.Width;
                        var rectangle = new Rectangle(offset, row, Item.Size.X, Item.Size.Y);

                        var directionalImage = fullImage.Clone(x => x.Crop(rectangle));
                        using var directionalStream = new MemoryStream();

                        directionalImage.SaveAsPng(directionalStream);
                        directionalStream.Seek(0, SeekOrigin.Begin);

                        var directionalBitmap = new Bitmap(directionalStream);

                        directionalImages[i] = directionalBitmap;

                        delaysIterated += totalDelays;
                    }

                    Frames.North = directionalImages[0];
                    Frames.South = directionalImages[1];
                    Frames.East = directionalImages[2];
                    Frames.West = directionalImages[3];
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"Unknown direction type {state.Image.State.Directions}");
            }
        }

        public void Dispose()
        {
            _emptyStream.Dispose();
        }
    }
}
