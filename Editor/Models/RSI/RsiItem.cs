using Importer.RSI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Editor.Models.RSI
{
    public class RsiItem
    {
        public RsiItem(Rsi? rsi = null)
        {
            Rsi = rsi ?? new Rsi();
        }

        public Rsi Rsi { get; }

        public RsiSize Size => Rsi.Size;

        public void LoadImage(int index, Image<Rgba32> image)
        {
            Rsi.States[index].LoadImage(image, Size);
        }

        public void AddState(RsiImage image)
        {
            Rsi.States.Add(image.State);
        }

        public void InsertState(int index, RsiImage image)
        {
            Rsi.States.Insert(index, image.State);
        }

        public void RemoveState(int index)
        {
            Rsi.States.RemoveAt(index);
        }

        public void RemoveState(RsiState state)
        {
            var index = Rsi.States.IndexOf(state);

            if (index != -1)
            {
                RemoveState(index);
            }
        }
    }
}
