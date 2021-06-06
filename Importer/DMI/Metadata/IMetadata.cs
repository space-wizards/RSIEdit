using Importer.RSI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Importer.DMI.Metadata
{
    public interface IMetadata
    {
        public double Version { get; }

        public int Width { get; }

        public int Height { get; }

        public Rsi ToRsi(Image<Rgba32> image);
    }
}
