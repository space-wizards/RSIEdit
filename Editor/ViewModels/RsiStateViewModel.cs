using Avalonia.Media.Imaging;
using Editor.Models.RSI;
using Importer.RSI;
using Microsoft.Toolkit.Diagnostics;

namespace Editor.ViewModels
{
    public class RsiStateViewModel : ViewModelBase
    {
        public RsiStateViewModel(RsiImage image)
        {
            Guard.IsNotNull(image, "image");
            Image = image;
        }

        public RsiImage Image { get; }

        public RsiState State => Image.State;

        public string Name
        {
            get => State.Name;
            set => State.Name = value;
        }

        public Bitmap Bitmap => Image.Bitmap;
    }
}
