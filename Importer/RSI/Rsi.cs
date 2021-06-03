using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Importer.RSI
{
    [PublicAPI]
    public class Rsi
    {
        private static readonly PngEncoder Encoder = new();

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

        public async Task SaveTo(string dmiPath, string rsiFolder)
        {
            await foreach (var loaded in LoadImages(dmiPath))
            {
                var stateName = States[loaded.index].Name;
                await loaded.image.SaveAsync($"{rsiFolder}{Path.DirectorySeparatorChar}{stateName}");
            }
        }

        public async IAsyncEnumerable<(Image<Rgba32> image, int index)> LoadImages(string dmiPath)
        {
            using var dmi = await Image.LoadAsync<Rgba32>(dmiPath);

            for (var y = 0; y < dmi.Height; y += Size.Y)
            {
                for (var x = 0; x < dmi.Width; x += Size.X)
                {
                    var xIndex = x / Size.X;
                    var yIndex = (y / Size.Y) * (dmi.Width / Size.X);
                    var index = xIndex + yIndex;

                    if (index >= States.Count)
                    {
                        yield break;
                    }

                    var rectangle = new Rectangle(x, y, Size.X, Size.Y);
                    var crop = dmi.Clone(c => c.Crop(rectangle));

                    yield return (crop, index);
                }
            }
        }

        public async IAsyncEnumerable<(Stream stream, int index)> LoadStreams(string dmiPath)
        {
            await foreach (var (image, index) in LoadImages(dmiPath))
            {
                var stream = new MemoryStream();
                await image.SaveAsync(stream, Encoder);
                stream.Seek(0, SeekOrigin.Begin);

                yield return (stream, index);
            }
        }
    }
}
