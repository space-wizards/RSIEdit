using System.Collections.Generic;
using System.Text.Json.Serialization;
using Importer.Directions;
using JetBrains.Annotations;
using SixLabors.ImageSharp;

namespace Importer.RSI
{
    [PublicAPI]
    public class RsiState
    {
        public RsiState(
            string name,
            DirectionType directions = DirectionType.None,
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
        public DirectionType Directions { get; set; }

        [JsonPropertyName("delays")]
        public List<List<float>>? Delays { get; set; }

        [JsonPropertyName("flags")]
        public Dictionary<object, object>? Flags { get; set; }

        public int FirstFrameIndexFor(RsiSize size, Direction direction)
        {
            var directionIndex = (int) direction;

            if (Delays == null)
            {
                return directionIndex;
            }

            if (directionIndex >= Delays.Count)
            {
                return -1;
            }

            var currentFrame = 0;
            for (var i = 0; i < directionIndex; i++)
            {
                var frames = Delays[i].Count;
                currentFrame += frames;
            }

            return currentFrame;
        }

        public (int x, int y) CoordinatesForFrame(RsiSize size, int index, int fileMultipleX)
        {
            var x = index * size.X;
            var fileWidth = fileMultipleX * size.X;
            var y = x / fileWidth * size.Y;
            x = x % fileWidth;

            return (x, y);
        }

        public Rectangle? FirstFrameFor(RsiSize size, Direction direction, int fileWidth, int fileHeight)
        {
            var index = FirstFrameIndexFor(size, direction);

            if (index == -1)
            {
                return null;
            }

            var fileMultipleX = fileWidth / size.X;
            var (x, y) = CoordinatesForFrame(size, index, fileMultipleX);

            if (x >= fileWidth || y >= fileHeight)
            {
                return null;
            }

            return new Rectangle(x, y, size.X, size.Y);
        }
    }
}
