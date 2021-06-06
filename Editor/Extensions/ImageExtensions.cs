using System.IO;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Editor.Extensions
{
    public static class ImageExtensions
    {
        public static Bitmap ToBitmap<T>(this Image<T> image) where T : unmanaged, IPixel<T>
        {
            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new Bitmap(stream);
        }
    }
}
