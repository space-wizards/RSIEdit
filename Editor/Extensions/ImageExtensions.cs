using System.IO;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Editor.Extensions
{
    public static class ImageExtensions
    {
        public static Bitmap ToBitmap<T>(this Image<T> image, ResizeOptions? resizeOptions = null) where T : unmanaged, IPixel<T>
        {
            if (resizeOptions != null)
            {
                image = image.Clone(x => x.Resize(resizeOptions));
            }

            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new Bitmap(stream);
        }
    }
}
