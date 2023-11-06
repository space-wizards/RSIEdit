using Editor.Models.RSI;
using Microsoft.Toolkit.Diagnostics;
using ReactiveUI;

namespace Editor.ViewModels;

public class RsiStateViewModel : ViewModelBase
{
    private string _name;

    public RsiStateViewModel(RsiImage image)
    {
        Guard.IsNotNull(image, "image");
        Image = image;
        _name = image.State.Name;
    }

    public RsiImage Image { get; }

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            Image.State.Name = value;
        }
    }
}
