using Avalonia.Interactivity;
using Editor.Models;

namespace Editor.Views.Events
{
    public class PreferencesChangedEvent : RoutedEventArgs
    {
        public PreferencesChangedEvent(Preferences preferences)
        {
            Preferences = preferences;
        }

        public Preferences Preferences { get; }
    }
}
