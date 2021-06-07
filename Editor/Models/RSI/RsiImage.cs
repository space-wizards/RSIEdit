using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Importer.RSI;
using JetBrains.Annotations;

namespace Editor.Models.RSI
{
    public class RsiImage : INotifyPropertyChanged
    {
        private Bitmap _preview;

        public RsiImage(RsiState state, Bitmap preview)
        {
            State = state;
            _preview = preview;
        }

        public RsiState State { get; }

        public Bitmap Preview
        {
            get => _preview;
            set
            {
                _preview = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
