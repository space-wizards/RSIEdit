using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Importer.DMI;
using Importer.RSI;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Editor.Models.RSI
{
    public class RsiImage : INotifyPropertyChanged
    {
        private Bitmap _bitmap;
        private Bitmap _south;
        private Bitmap _north;
        private Bitmap _east;
        private Bitmap _west;

        public RsiImage(RsiSize size, RsiState state, Bitmap bitmap)
        {
            Size = size;
            State = state;
            _bitmap = bitmap;

            _north = bitmap;
            _south = bitmap;
            _east = bitmap;
            _west = bitmap;

            RefreshFrames(size, state, bitmap);
        }

        private RsiSize Size { get; }

        public RsiState State { get; }

        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _bitmap = value;
                OnPropertyChanged();

                RefreshFrames(Size, State, value);
            }
        }

        public Bitmap South
        {
            get => _south;
            set
            {
                _south = value;
                OnPropertyChanged();
            }
        }

        public Bitmap North
        {
            get => _north;
            set
            {
                _north = value;
                OnPropertyChanged();
            }
        }

        public Bitmap East
        {
            get => _east;
            set
            {
                _east = value;
                OnPropertyChanged();
            }
        }

        public Bitmap West
        {
            get => _west;
            set
            {
                _west = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RefreshFrames(RsiSize size, RsiState state, Bitmap bitmap)
        {
            switch (state.Directions)
            {
                case DirectionTypes.None:
                    South = bitmap;
                    North = bitmap;
                    East = bitmap;
                    West = bitmap;
                    break;
                case DirectionTypes.Cardinal:
                case DirectionTypes.Diagonal:
                {
                    if (state.Delays == null)
                    {
                        South = bitmap;
                        North = bitmap;
                        East = bitmap;
                        West = bitmap;
                        return;
                    }

                    using var stream = new MemoryStream();
                    bitmap.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    var fullImage = Image.Load<Rgba32>(stream, new PngDecoder());

                    var delaysIterated = 0;

                    var directionalImages = new Bitmap[4];

                    for (var i = 0; i < 4; i++)
                    {
                        var delays = state.Delays[i].Count;
                        var totalWidth = delaysIterated * size.X;
                        var row = totalWidth / fullImage.Width;
                        var offset = totalWidth % fullImage.Width;

                        var directionalImage = fullImage.Clone(x => x.Crop(new Rectangle(offset, row, size.X, size.Y)));
                        using var directionalStream = new MemoryStream();

                        directionalImage.SaveAsPng(directionalStream);
                        directionalStream.Seek(0, SeekOrigin.Begin);

                        var directionalBitmap = new Bitmap(directionalStream);

                        directionalImages[i] = directionalBitmap;

                        delaysIterated += delays;
                    }

                    North = directionalImages[0];
                    South = directionalImages[1];
                    East = directionalImages[2];
                    West = directionalImages[3];
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"Unknown direction type {state.Directions}");
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
