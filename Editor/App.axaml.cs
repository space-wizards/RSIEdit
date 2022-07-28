using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Editor.Models;
using Editor.ViewModels;
using Editor.Views;
using Splat;

namespace Editor;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private new void RegisterServices()
    {
        Locator.CurrentMutable.RegisterLazySingleton(() =>
        {
            Preferences preferences;
            var filePath = "preferences.json";

            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    preferences = JsonSerializer.Deserialize<Preferences>(json) ?? new Preferences();
                }
                catch (Exception e)
                {
                    Logger.Sink.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
                    preferences = new Preferences();
                    File.WriteAllText(filePath, string.Empty);
                }
            }
            else
            {
                preferences = new Preferences();
            }

            return preferences;
        });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        RegisterServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}