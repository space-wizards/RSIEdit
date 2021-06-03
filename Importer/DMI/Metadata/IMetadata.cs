using Importer.RSI;

namespace Importer.DMI.Metadata
{
    public interface IMetadata
    {
        public double Version { get; }

        public int Width { get; }

        public int Height { get; }

        public Rsi ToRsi();
    }
}
