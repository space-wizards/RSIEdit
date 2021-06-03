using System.Collections.Generic;
using System.Linq;
using Importer.RSI;

namespace Importer.DMI.Metadata
{
    public class Metadata : IMetadata
    {
        public Metadata(Version version, List<DmiState> states)
        {
            Version = version.VersionNumber;
            Width = version.Width;
            Height = version.Height;
            States = states;
        }

        public double Version { get; }

        public int Width { get; }

        public int Height { get; }

        public List<DmiState> States { get; }

        public Rsi ToRsi()
        {
            var rsiStates = States.Select(s => s.ToRsiState()).ToList();
            return new Rsi(Width, Height, rsiStates);
        }
    }
}
