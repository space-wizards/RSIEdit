using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Importer.Directions;
using JetBrains.Annotations;
using Microsoft.Toolkit.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Importer.RSI
{
    [PublicAPI]
    public class RsiState : IDisposable
    {
        private static readonly char[] InvalidFilenameChars = Path.GetInvalidFileNameChars();

        public RsiState(
            string name,
            DirectionType directions = DirectionType.None,
            List<List<float>>? delays = null,
            Dictionary<object, object>? flags = null,
            RsiSize? size = null,
            Image<Rgba32>[,]? frames = null,
            string? invalidCharacterReplace = "_")
        {
            if (invalidCharacterReplace != null)
            {
                name = string.Join(invalidCharacterReplace, name.Split(Path.GetInvalidFileNameChars()));
            }

            Name = name;
            Directions = directions;
            Delays = delays;
            Flags = flags;
            Size = size ?? new RsiSize(32, 32);

            DelayLength = Delays is {Count: > 0} ? Delays[0].Count : 1;
            Frames = frames ?? new Image<Rgba32>[8, DelayLength];

            Guard.IsEqualTo(Frames.Length, 8 * DelayLength, "Frames.Length");
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("directions")]
        public DirectionType Directions { get; set; }

        [JsonPropertyName("delays")]
        public List<List<float>>? Delays { get; set; }

        [JsonPropertyName("flags")]
        public Dictionary<object, object>? Flags { get; set; }

        [JsonIgnore]
        public int DelayLength { get; private set; }

        [JsonIgnore]
        public RsiSize Size { get; private set; }

        [JsonIgnore]
        public Image<Rgba32>?[,] Frames { get; private set; }

        public static (int rows, int columns) GetRowsAndColumns(int images)
        {
            var sqrt = Math.Sqrt(images);
            var rows = (int) Math.Ceiling(sqrt);
            var columns = (int) Math.Round(sqrt);

            return (rows, columns);
        }

        public Image<Rgba32> GetFullImage()
        {
            var totalImages = Frames.Cast<Image<Rgba32>>().Count(x => x != null);
            var (rows, columns) = GetRowsAndColumns(totalImages);
            var image = new Image<Rgba32>(Size.X * columns, Size.Y * rows);

            var currentX = 0;
            var currentY = 0;

            foreach (var frame in Frames)
            {
                if (frame == null)
                {
                    continue;
                }

                var point = new Point(currentX, currentY);
                image.Mutate(x => x.DrawImage(frame, point, 1));

                currentX += Size.X;

                if (currentX >= image.Width)
                {
                    currentX = 0;
                    currentY += Size.Y;
                }
            }

            return image;
        }

        public int FirstFrameIndexFor(RsiSize size, Direction direction)
        {
            var directionIndex = (int) direction;

            if (Delays == null)
            {
                return directionIndex;
            }

            if (directionIndex >= Delays.Count)
            {
                return (int) direction;
            }

            var currentFrame = 0;
            for (var i = 0; i < directionIndex; i++)
            {
                var frames = Delays[i].Count;
                currentFrame += frames;
            }

            return currentFrame;
        }

        public Image<Rgba32>?[] FirstImageFor(Direction direction)
        {
            var image = new Image<Rgba32>?[DelayLength];

            for (var i = 0; i < Frames.GetUpperBound((int) direction); i++)
            {
                image[i] = Frames[(int) direction, i];
            }

            return image;
        }

        public Image<Rgba32>? FirstFrameFor(Direction direction)
        {
            return FirstImageFor(direction)[0];
        }

        public Image<Rgba32>?[] FirstFrameForAll(DirectionType directionType)
        {
            var frames = new Image<Rgba32>?[8];

            for (var i = 0; i < (int) directionType; i++)
            {
                var direction = (Direction) i;
                frames[i] = FirstFrameFor(direction);
            }

            return frames;
        }

        public void LoadImage(Image<Rgba32> image)
        {

        }

        public void Dispose()
        {
            foreach (var image in Frames)
            {
                image?.Dispose();
            }

            Array.Clear(Frames, 0, Frames.Length);
        }
    }
}
