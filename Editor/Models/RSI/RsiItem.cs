using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Editor.Models.RSI
{
    public class RsiItem
    {
        private const int CurrentRsiVersion = 1;

        [JsonConstructor]
        public RsiItem(
            int version,
            RsiSize size,
            List<RsiState>? states = null,
            string license = "",
            string copyright = "")
        {
            Version = version;
            Size = size;
            States = states ?? new List<RsiState>();
            License = license;
            Copyright = copyright;
        }

        public RsiItem(
            int version = CurrentRsiVersion,
            int x = 32,
            int y = 32,
            IEnumerable<RsiState>? states = null,
            string license = "",
            string copyright = "")
            : this(version, new RsiSize(x, y), states?.ToList(), license, copyright)
        {
        }

        public RsiItem(
            int version,
            int size,
            IEnumerable<RsiState>? states = null,
            string license = "",
            string copyright = "")
            : this(version, new RsiSize(size, size), states?.ToList(), license, copyright)
        {
        }

        [JsonPropertyName("version")]
        public int Version { get; }

        [JsonPropertyName("size")]
        public RsiSize Size { get; }

        [JsonPropertyName("states")]
        public List<RsiState> States { get; }

        [JsonPropertyName("license")]
        public string? License { get; }

        [JsonPropertyName("copyright")]
        public string? Copyright { get; }
    }
}
