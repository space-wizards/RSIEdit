using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;

namespace Editor.Models.RSI
{
    public class RsiState
    {
        public RsiState(
            string name,
            Bitmap image,
            RsiStateDirections directions = RsiStateDirections.None,
            List<List<float>>? delays = null,
            Dictionary<object, object>? flags = null)
        {
            Name = name;
            Image = image;
            Directions = directions;
            Delays = delays;
            Flags = flags;
        }

        [JsonPropertyName("name")]
        public string Name { get; }

        public Bitmap Image { get; internal set; }

        [JsonPropertyName("directions")]
        public RsiStateDirections Directions { get; }

        [JsonPropertyName("delays")]
        public List<List<float>>? Delays { get; }

        [JsonPropertyName("flags")]
        public Dictionary<object, object>? Flags { get; }
    }
}
