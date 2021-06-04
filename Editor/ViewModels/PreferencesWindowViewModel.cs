using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Editor.Models;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class PreferencesWindowViewModel : ViewModelBase
    {
        private string? _defaultLicense;
        private string? _defaultCopyright;

        public PreferencesWindowViewModel(Preferences preferences)
        {
            Preferences = preferences;
            DefaultLicense = Preferences.DefaultLicense;
            DefaultCopyright = Preferences.DefaultCopyright;
        }

        private Preferences Preferences { get; }

        public Interaction<Preferences, Unit> SaveAction { get; } = new();

        public Interaction<Unit, Unit> CancelAction { get; } = new();

        public string? DefaultLicense
        {
            get => _defaultLicense;
            set
            {
                this.RaiseAndSetIfChanged(ref _defaultLicense, value);
                Preferences.DefaultLicense = value;
            }
        }

        public string? DefaultCopyright
        {
            get => _defaultCopyright;
            set
            {
                this.RaiseAndSetIfChanged(ref _defaultCopyright, value);
                Preferences.DefaultCopyright = value;
            }
        }

        public async Task Save()
        {
            await SaveAction.Handle(Preferences);
        }

        public async Task Cancel()
        {
            await CancelAction.Handle(Unit.Default);
        }
    }
}
