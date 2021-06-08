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
        private bool _minifyJson;

        public PreferencesWindowViewModel(Preferences preferences)
        {
            Preferences = preferences;
            DefaultLicense = Preferences.DefaultLicense;
            DefaultCopyright = Preferences.DefaultCopyright;
            MinifyJson = Preferences.MinifyJson;
            EasterEggs = Preferences.EasterEggs;
        }

        public Preferences Preferences { get; }

        public Interaction<Preferences, Unit> SaveAction { get; } = new();

        public Interaction<Preferences, Unit> CancelAction { get; } = new();

        public string? DefaultLicense
        {
            get => _defaultLicense;
            set => this.RaiseAndSetIfChanged(ref _defaultLicense, value);
        }

        public string? DefaultCopyright
        {
            get => _defaultCopyright;
            set => this.RaiseAndSetIfChanged(ref _defaultCopyright, value);
        }

        public bool MinifyJson
        {
            get => _minifyJson;
            set => this.RaiseAndSetIfChanged(ref _minifyJson, value);
        }

        public bool EasterEggs
        {
            get => _easterEggs;
            set => this.RaiseAndSetIfChanged(ref _easterEggs, value);
        }

        public async Task Save()
        {
            Preferences.DefaultLicense = DefaultLicense;
            Preferences.DefaultCopyright = DefaultCopyright;
            Preferences.MinifyJson = MinifyJson;
            Preferences.EasterEggs = EasterEggs;

            await SaveAction.Handle(Preferences);
        }

        public async Task Cancel()
        {
            await CancelAction.Handle(Preferences);
        }
    }
}
