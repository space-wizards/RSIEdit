using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia.Media.Imaging;
using Importer.RSI;

namespace Editor.Models.RSI
{
    public class RsiItem
    {
        private readonly List<RsiImage> _images;

        public RsiItem(Rsi? rsi = null)
        {
            Rsi = rsi ?? new Rsi();
            _images = new List<RsiImage>(new RsiImage[Rsi.States.Count]);
        }

        private Rsi Rsi { get; }

        public double Version => Rsi.Version;

        public RsiSize Size => Rsi.Size;

        public IReadOnlyList<RsiImage> Images => _images;

        public string? License => Rsi.License;

        public string? Copyright => Rsi.Copyright;

        public bool TryLoadImages(string folder, [NotNullWhen(false)] out string? error)
        {
            for (var i = 0; i < Rsi.States.Count; i++)
            {
                var state = Rsi.States[i];
                var statePath = $"{folder}{Path.DirectorySeparatorChar}{state.Name}.png";

                if (!File.Exists(statePath))
                {
                    error = $"Missing state found in meta.json:\n{statePath}";
                    return false;
                }

                var bitmap = new Bitmap(statePath);
                var image = new RsiImage(state, bitmap);
                _images[i] = image;
            }

            error = null;
            return true;
        }

        public void LoadImage(int index, Bitmap image)
        {
            var state = Rsi.States[index];
            _images[index] = new RsiImage(state, image);
        }

        public void InsertState(int index, RsiImage image)
        {
            Rsi.States.Insert(index, image.State);
            _images.Insert(index, image);
        }

        public void RemoveState(int index)
        {
            Rsi.States.RemoveAt(index);
            _images.RemoveAt(index);
        }

        public void RemoveState(RsiState state)
        {
            var index = Rsi.States.IndexOf(state);

            if (index != -1)
            {
                RemoveState(index);
            }
        }
    }
}
