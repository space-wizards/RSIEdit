using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class RsiFramesViewModel : ViewModelBase
    {
        private Bitmap _south;
        private Bitmap _north;
        private Bitmap _east;
        private Bitmap _west;

        public RsiFramesViewModel(Bitmap south, Bitmap north, Bitmap east, Bitmap west)
        {
            _south = south;
            _north = north;
            _east = east;
            _west = west;
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
    }
}
