using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Size = Avalonia.Size;

namespace Editor.Extensions;

public static class ImageExtensions
{
    public static Bitmap ToBitmap<T>(this Image<T> image, ResizeOptions? resizeOptions = null) where T : unmanaged, IPixel<T>
    {
        var dispose = false;
        if (resizeOptions != null)
        {
            dispose = true;
            image = image.Clone(x => x.Resize(resizeOptions));
        }

        if (!image.DangerousTryGetSinglePixelMemory(out var span))
            throw new InvalidOperationException("Image is too large!");

        var bitmap = SpanToBitmap<T>(span.Span, image.Width, image.Height);

        if (dispose)
            image.Dispose();
            
        return bitmap;
    }

    public static unsafe Bitmap SpanToBitmap<T>(ReadOnlySpan<T> span, int width, int height) where T : unmanaged
    {
        fixed (T* px = span)
        {
            var (pf, af) = default(T) switch
            {
                Rgba32 => (PixelFormat.Rgba8888, AlphaFormat.Unpremul),
                _ => throw new InvalidOperationException("Unsupported pixel format!")
            };

            return new Bitmap(
                pf, af, (IntPtr)px,
                PixelSize.FromSizeWithDpi(new Size(width, height), 96),
                new Vector(96, 96),
                sizeof(T) * width);
        }
    }
}