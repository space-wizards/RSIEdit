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
        private bool _easterEggs;

        public PreferencesWindowViewModel(Preferences preferences)
        {
            Preferences = preferences;
            DefaultLicense = Preferences.DefaultLicense;
            DefaultCopyright = Preferences.DefaultCopyright;
            EasterEggs = Preferences.EasterEggs;
        }

        private Preferences Preferences { get; }

        public Interaction<Preferences, Unit> SaveAction { get; } = new();

        public Interaction<Preferences, Unit> CancelAction { get; } = new();

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

        public bool EasterEggs
        {
            get => _easterEggs;
            set
            {
                this.RaiseAndSetIfChanged(ref _easterEggs, value);
                Preferences.EasterEggs = value;
            }
        }

        public async Task Save()
        {
            await SaveAction.Handle(Preferences);
        }

        public async Task Cancel()
        {
            await CancelAction.Handle(Preferences);
        }
    }
}
