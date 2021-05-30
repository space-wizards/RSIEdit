using System.Text.Json.Serialization;

namespace Editor.Models.RSI
{
    public class RsiSize
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
    }
}
