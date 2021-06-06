using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SixLabors.ImageSharp;

namespace Importer.RSI
{
    [PublicAPI]
    public class Rsi : IDisposable
    {
        public const double CurrentRsiVersion = 1;

        [JsonConstructor]
        public Rsi(
            double version,
            RsiSize size,
            List<RsiState>? states = null,
            string? license = null,
            string? copyright = null)
        {
            Version = version;
            Size = size;
            States = states ?? new List<RsiState>();
            License = license;
            Copyright = copyright;
        }

        public Rsi(
            double version = CurrentRsiVersion,
            int x = 32,
            int y = 32,
            List<RsiState>? states = null,
            string? license = null,
            string? copyright = null)
            : this(version, new RsiSize(x, y), states?.ToList(), license, copyright)
        {
        }

        [JsonPropertyName("name")]
        public double Version { get; set; }

        [JsonPropertyName("size")]
        public RsiSize Size { get; }

        [JsonPropertyName("states")]
        public List<RsiState> States { get; set; }

        [JsonPropertyName("license")]
        public string? License { get; set; }

        [JsonPropertyName("copyright")]
        public string? Copyright { get; set; }

        public async Task SaveTo(string rsiFolder)
        {
            foreach (var state in States)
            {
                var image = state.GetFullImage();

                await image.SaveAsync($"{rsiFolder}{Path.DirectorySeparatorChar}{state.Name}.png");
            }
        }

        public void Dispose()
        {
            foreach (var state in States)
            {
                state.Dispose();
            }
        }
    }
}
