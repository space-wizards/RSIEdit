using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SpaceWizards.RsiLib.RSI;
using SixLabors.ImageSharp.PixelFormats;

namespace Editor.Extensions;

public static class RsiStateExtensions
{
    public static async Task LoadImage(this RsiState state, Bitmap bitmap, RsiSize size)
    {
        await using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var frame = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(memoryStream);

        state.LoadImage(frame, size);
    }
}