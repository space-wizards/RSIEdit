using Avalonia.Media.Imaging;
using Editor.Models.RSI;

namespace Editor.ViewModels
{
    public class RsiStateViewModel : ViewModelBase
    {
        public RsiStateViewModel(RsiState state)
        {
            State = state;
        }

        internal RsiState State { get; }

        public string Name => State.Name;

        public Bitmap Image => State.Image;
    }
}
