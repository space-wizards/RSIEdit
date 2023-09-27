using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;

namespace Editor;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Logger.Sink?.Log(LogEventLevel.Fatal, "MAIN", null, e.ToString());

            if (Assembly.GetEntryAssembly() is { } assembly)
            {
                File.WriteAllTextAsync(Path.Join(assembly.Location, "crash_report.txt"), e.ToString());
            }

            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
