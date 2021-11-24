using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Editor.Views.Events;

public class SaveFileDialogEvent : RoutedEventArgs
{
    public SaveFileDialogEvent(SaveFileDialog dialog, Image<Rgba32> png)
    {
        Dialog = dialog;
        Png = png;
    }
    
    public SaveFileDialog Dialog { get; }
    public Image<Rgba32> Png { get; }
}
