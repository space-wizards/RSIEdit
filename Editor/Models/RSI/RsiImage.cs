using Avalonia.Media.Imaging;
using Importer.RSI;

namespace Editor.Models.RSI
{
    public class RsiImage
    {
        public RsiImage(RsiState state, Bitmap bitmap)
        {
            State = state;
            Bitmap = bitmap;
        }

        public RsiState State { get; }

        public Bitmap Bitmap { get; }
    }
}
