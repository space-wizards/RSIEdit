using System.Collections.Generic;
using JetBrains.Annotations;

namespace Importer.RSI
{
    [PublicAPI]
    public class Rsi
    {
        public const double CurrentRsiVersion = 1;

        public Rsi(
            double version = CurrentRsiVersion,
            int x = 32,
            int y = 32,
            List<RsiState>? states = null,
            string? license = "",
            string? copyright = "")
        {
            Version = version;
            X = x;
            Y = y;
            States = states ?? new List<RsiState>();
            License = license;
            Copyright = copyright;
        }

        public Rsi(
            int x = 32,
            int y = 32,
            List<RsiState>? states = null,
            string? license = "",
            string? copyright = "")
            : this(CurrentRsiVersion, x, y, states, license, copyright)
        {
        }

        public double Version { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public List<RsiState> States { get; set; }

        public string? License { get; set; }

        public string? Copyright { get; set; }
    }
}
