using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Editor.Extensions;
using Editor.Models.RSI;
using SpaceWizards.RsiLib.Directions;
using SpaceWizards.RsiLib.RSI;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ImageExtensions = Editor.Extensions.ImageExtensions;

namespace Editor.ViewModels;

public class RsiItemViewModel : ViewModelBase, IDisposable
{
    private const int DeletedBufferSize = 200;
    private const int RestoredBufferSize = 200;

    private static readonly ResizeOptions PreviewResizeOptions = new()
    {
        Mode = ResizeMode.Max,
        Position = AnchorPositionMode.Center,
        Size = new Size(96, 96),
        Sampler = KnownResamplers.NearestNeighbor
    };

    public static readonly List<string> ValidExtensions = new()
    {
        "gif",
        "png"
    };

    private readonly MemoryStream _emptyStream = new();
    private readonly Bitmap _blankFrame;

    private ObservableCollection<RsiStateViewModel> _selectedStates = new();
    private bool _hasStateSelected;
    private bool _hasOneStateSelected;
    private string _title;
    private ComboBoxItem? _selectedLicense;

    public RsiItemViewModel(string title = "New Rsi", RsiItem? item = null)
    {
        _title = title;
        Item = item ?? new RsiItem();

        SelectedStates.CollectionChanged += OnStateModified;

        _blankFrame = CreateEmptyBitmap();
        Frames = new RsiFramesViewModel(_blankFrame, null);

        foreach (var state in Item.Rsi.States)
        {
            var firstFrame = state.Frames[0, 0];
            var preview = firstFrame == null
                ? _blankFrame
                : firstFrame.ToBitmap(PreviewResizeOptions);
            var image = new RsiImage(state, preview);

            States.Add(new RsiStateViewModel(image));
        }
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public bool Modified { get; private set; }

    public string? SaveFolder { get; set; }

    public Interaction<Unit, string> ImportImageInteraction { get; } = new();

    public Interaction<Image<Rgba32>, Unit> ExportPngInteraction { get; } = new();

    public Interaction<ErrorWindowViewModel, Unit> ErrorDialog { get; } = new();

    public Interaction<RsiItemViewModel, Unit> CloseInteraction { get; } = new();

    public RsiItem Item { get; }

    public int SizeX
    {
        get => Item.Size.X;
        set
        {
            if (Item.Size.X == value)
                return;

            this.RaisePropertyChanging();
            Item.Rsi.Size = Item.Size with { X = value };
            this.RaisePropertyChanged();
        }
    }

    public int SizeY
    {
        get => Item.Size.Y;
        set
        {
            if (Item.Size.Y == value)
                return;

            this.RaisePropertyChanging();
            Item.Rsi.Size = Item.Size with { Y = value };
            this.RaisePropertyChanged();
        }
    }

    public ObservableCollection<RsiStateViewModel> States { get; } = new();

    public ObservableCollection<RsiStateViewModel> SelectedStates
    {
        get => _selectedStates;
        set => this.RaiseAndSetIfChanged(ref _selectedStates, value);
    }

    public bool HasStateSelected
    {
        get => _hasStateSelected;
        set => this.RaiseAndSetIfChanged(ref _hasStateSelected, value);
    }

    public bool HasOneStateSelected
    {
        get => _hasOneStateSelected;
        set => this.RaiseAndSetIfChanged(ref _hasOneStateSelected, value);
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

            var valueLicense = (string?) value.Content;

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

    public void OnStateModified(object? obj, NotifyCollectionChangedEventArgs args)
    {
        HasStateSelected = SelectedStates.Count != 0;
        HasOneStateSelected = SelectedStates.Count == 1;
        RefreshFrames();
    }

    public async Task CreateNewState(string? pngFilePath = null)
    {
        var state = new RsiState
        {
            ImagePath = pngFilePath
        };

        Bitmap bitmap;
        if (string.IsNullOrEmpty(pngFilePath))
        {
            bitmap = CreateEmptyBitmap();
        }
        else
        {
            state.Name = Path.GetFileNameWithoutExtension(pngFilePath);

            try
            {
                bitmap = new Bitmap(pngFilePath);
            }
            catch (Exception e)
            {
                Logger.Sink?.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
                var errorVm = new ErrorWindowViewModel($"Error creating a state from file\n{pngFilePath}");
                await ErrorDialog.Handle(errorVm);
                return;
            }
        }

        var image = new RsiImage(state, bitmap);
        var vm = new RsiStateViewModel(image);

        AddState(vm);
        await vm.Image.State.LoadImage(bitmap, Item.Size);
        Modified = true;
    }

    private bool IsExtensionValid(string filepath)
    {
        foreach (var ext in ValidExtensions)
        {
            if (filepath.EndsWith(ext))
                return true;
        }

        return false;
    }

    public async Task ImportImage()
    {
        if (!HasOneStateSelected)
        {
            return;
        }

        var path = await ImportImageInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (!IsExtensionValid(path))
        {
            var vm = new ErrorWindowViewModel($"File {path} is not a valid image");
            await ErrorDialog.Handle(vm);
            return;
        }

        if (path.EndsWith(".gif"))
        {
            Bitmap png;
            try
            {
                png = new Bitmap(path);
            }
            catch (Exception e)
            {
                Logger.Sink?.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
                var vm = new ErrorWindowViewModel($"Error opening file {path}");
                await ErrorDialog.Handle(vm);
                return;
            }

            var oldBitmap = SelectedStates[0].Image.Preview;
            if (oldBitmap != _blankFrame)
            {
                oldBitmap.Dispose();
            }

            SelectedStates[0].Image.Preview = png;
            var gif = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(path);
            SelectedStates[0].Image.State.LoadGif(gif);
        }
        else if (path.EndsWith(".png"))
        {
            Bitmap png;
            try
            {
                png = new Bitmap(path);
            }
            catch (Exception e)
            {
                Logger.Sink?.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
                var vm = new ErrorWindowViewModel($"Error opening file {path}");
                await ErrorDialog.Handle(vm);
                return;
            }

            var oldBitmap = SelectedStates[0].Image.Preview;
            if (oldBitmap != _blankFrame)
            {
                oldBitmap.Dispose();
            }

            SelectedStates[0].Image.Preview = png;
            await SelectedStates[0].Image.State.LoadImage(png, Item.Size);
        }

        RefreshFrames();
        UpdateImageState(SelectedStates[0]);
    }

    public async Task ExportPng()
    {
        var png = SelectedStates[0].Image.State.GetFullImage(Item.Size);

        await ExportPngInteraction.Handle(png);
    }

    public void DeleteSelectedStates()
    {
        if (_selectedStates.Count == 0)
        {
            return;
        }

        var lastIndex = -1;
        foreach (var state in _selectedStates.ToArray())
        {
            TryDelete(state, out lastIndex);
        }

        ReselectState(lastIndex);
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

    public void UpdateImageState(RsiStateViewModel vm)
    {
        Item.UpdateImageState(States.IndexOf(vm), vm.Image);
        Modified = true;
    }

    public bool TryDelete(RsiStateViewModel stateVm, out int index)
    {
        Item.RemoveState(stateVm.Image.State);

        index = States.IndexOf(stateVm);
        var removed = States.Remove(stateVm);

        if (!removed)
        {
            return false;
        }

        if (SelectedStates.Contains(stateVm))
        {
            SelectedStates.Remove(stateVm);
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

    public bool TryRedoDelete(out int index)
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
            SelectedStates.Add(States[nextSelectedState.Value]);;
        }
    }

    private void RefreshFrames()
    {
        if (SelectedStates.Count == 0)
        {
            for (var i = 0; i < 8; i++)
            {
                Frames.Set((Direction) i, _blankFrame);
            }

            Frames.SetDirections(null);

            return;
        }

        foreach (var state in SelectedStates)
        {
            var image = state.Image;
            var rsiState = image.State;

            for (var direction = 0; direction < (int) rsiState.Directions; direction++)
            {
                var frame = rsiState.Frames[direction, 0]?.ToBitmap(PreviewResizeOptions) ?? _blankFrame;
                Frames.Set((Direction) direction, frame);
            }

            Frames.SetDirections(rsiState.Directions);
        }

    }

    private Bitmap CreateEmptyBitmap()
    {
        var (width, height) = Item.Size;
        var bufLength = width * height;
        var buf = ArrayPool<Rgba32>.Shared.Rent(bufLength);
        Array.Clear(buf);
        var bitmap = ImageExtensions.SpanToBitmap<Rgba32>(buf.AsSpan(0, bufLength), width, height);
        ArrayPool<Rgba32>.Shared.Return(buf);

        return bitmap;
    }

    public void Save()
    {
        if (SaveFolder == null)
        {
            return;
        }

        Item.Rsi.SaveToFolder(SaveFolder);
    }

    public void Dispose()
    {
        _emptyStream.Dispose();
        Item.Rsi.Dispose();
    }
}
