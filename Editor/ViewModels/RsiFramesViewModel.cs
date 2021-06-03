using Avalonia.Media.Imaging;

namespace Editor.ViewModels
{
    public class RsiFramesViewModel : ViewModelBase
    {
        public RsiFramesViewModel(Bitmap south, Bitmap north, Bitmap east, Bitmap west)
        {
            South = south;
            North = north;
            East = east;
            West = west;
        }

        public Bitmap South { get; }

        public Bitmap North { get; }

        public Bitmap East { get; }

        public Bitmap West { get; }
    }
}
