using System;
using Avalonia.Media.Imaging;
using SpaceWizards.RsiLib.Directions;
using ReactiveUI;

namespace Editor.ViewModels;

public class RsiFramesViewModel : ViewModelBase
{
    private Bitmap _full;
    private Bitmap _south;
    private Bitmap _north;
    private Bitmap _east;
    private Bitmap _west;
    private Bitmap _southEast;
    private Bitmap _southWest;
    private Bitmap _northEast;
    private Bitmap _northWest;
    private bool _showFull;
    private bool _showCardinals;
    private bool _showDiagonals;

    public RsiFramesViewModel(Bitmap full, DirectionType? direction)
    {
        _full = full;
        _south = full;
        _north = full;
        _east = full;
        _west = full;
        _southEast = full;
        _southWest = full;
        _northEast = full;
        _northWest = full;

        SetDirections(direction);
    }

    public bool ShowFull
    {
        get => _showFull;
        set => this.RaiseAndSetIfChanged(ref _showFull, value);
    }

    public bool ShowCardinals
    {
        get => _showCardinals;
        set => this.RaiseAndSetIfChanged(ref _showCardinals, value);
    }

    public bool ShowDiagonals
    {
        get => _showDiagonals;
        set => this.RaiseAndSetIfChanged(ref _showDiagonals, value);
    }

    public Bitmap Full
    {
        get => _full;
        set => this.RaiseAndSetIfChanged(ref _full, value);
    }

    public Bitmap South
    {
        get => _south;
        set => this.RaiseAndSetIfChanged(ref _south, value);
    }

    public Bitmap North
    {
        get => _north;
        set => this.RaiseAndSetIfChanged(ref _north, value);
    }

    public Bitmap East
    {
        get => _east;
        set => this.RaiseAndSetIfChanged(ref _east, value);
    }

    public Bitmap West
    {
        get => _west;
        set => this.RaiseAndSetIfChanged(ref _west, value);
    }

    public Bitmap SouthEast
    {
        get => _southEast;
        set => this.RaiseAndSetIfChanged(ref _southEast, value);
    }

    public Bitmap SouthWest
    {
        get => _southWest;
        set => this.RaiseAndSetIfChanged(ref _southWest, value);
    }

    public Bitmap NorthEast
    {
        get => _northEast;
        set => this.RaiseAndSetIfChanged(ref _northEast, value);
    }

    public Bitmap NorthWest
    {
        get => _northWest;
        set => this.RaiseAndSetIfChanged(ref _northWest, value);
    }

    public void Set(Direction direction, Bitmap image)
    {
        switch (direction)
        {
            case Direction.South:
                Full = image;
                South = image;
                break;
            case Direction.North:
                North = image;
                break;
            case Direction.East:
                East = image;
                break;
            case Direction.West:
                West = image;
                break;
            case Direction.SouthEast:
                SouthEast = image;
                break;
            case Direction.SouthWest:
                SouthWest = image;
                break;
            case Direction.NorthEast:
                NorthEast = image;
                break;
            case Direction.NorthWest:
                NorthWest = image;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public void SetDirections(DirectionType? direction)
    {
        ShowFull = direction == DirectionType.None;
        ShowCardinals = direction == DirectionType.Cardinal || direction == DirectionType.Diagonal;
        ShowDiagonals = direction == DirectionType.Diagonal;
    }
}