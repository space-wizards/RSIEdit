using System.Collections.Generic;
using System.Text.Json.Serialization;
using Importer.DMI;
using JetBrains.Annotations;

namespace Importer.RSI
{
    [PublicAPI]
    public class RsiState
    {
        public RsiState(
            string name,
            DirectionTypes directions = DirectionTypes.None,
            List<List<float>>? delays = null,
            Dictionary<object, object>? flags = null)
        {
            Name = name;
            Directions = directions;
            Delays = delays;
            Flags = flags;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("directions")]
        public DirectionTypes Directions { get; set; }

        [JsonPropertyName("delays")]
        public List<List<float>>? Delays { get; set; }

        [JsonPropertyName("flags")]
        public Dictionary<object, object>? Flags { get; set; }
    }
}
