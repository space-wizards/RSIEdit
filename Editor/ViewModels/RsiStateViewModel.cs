using Editor.Models.RSI;
using Microsoft.Toolkit.Diagnostics;

namespace Editor.ViewModels
{
    public class RsiStateViewModel : ViewModelBase
    {
        public RsiStateViewModel(RsiImage image)
        {
            Guard.IsNotNull(image, "image");
            Image = image;
        }

        public RsiImage Image { get; }
    }
}
