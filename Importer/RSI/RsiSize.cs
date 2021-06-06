using System.Text.Json.Serialization;

namespace Importer.RSI
{
    public record RsiSize
    {
        public RsiSize(int x, int y)
        {
            X = x;
            Y = y;
        }

        [JsonPropertyName("x")]
        public int X { get; }

        [JsonPropertyName("y")]
        public int Y { get; }

        public (int x, int y) CoordinatesForFrame(int index, int fileWidth)
        {
            var x = index * X;
            var y = x / fileWidth * Y;
            x = x % fileWidth;

            return (x, y);
        }
    }
}
