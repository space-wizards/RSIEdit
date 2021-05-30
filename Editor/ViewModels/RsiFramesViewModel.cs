using Avalonia.Media.Imaging;
using Editor.Models.RSI;

namespace Editor.ViewModels
{
    public class RsiFramesViewModel : ViewModelBase
    {
        public RsiFramesViewModel(RsiState state)
        {
            State = state;
        }

        public RsiState State { get; }

        public Bitmap South => State.Image;

        public Bitmap North => State.Image;

        public Bitmap East => State.Image;

        public Bitmap West => State.Image;
    }
}
