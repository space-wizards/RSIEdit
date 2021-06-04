using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Importer.RSI;
using JetBrains.Annotations;

namespace Editor.Models.RSI
{
    public class RsiImage : INotifyPropertyChanged
    {
        private Bitmap _bitmap;

        public RsiImage(RsiState state, Bitmap bitmap)
        {
            State = state;
            _bitmap = bitmap;
        }

        public RsiState State { get; }

        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _bitmap = value;
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
