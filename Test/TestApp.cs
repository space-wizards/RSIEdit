using Avalonia.Controls.ApplicationLifetimes;
using Editor;
using Editor.Views;

namespace Test;

public class TestApp : App
{
    public ClassicDesktopStyleApplicationLifetime Lifetime()
    {
        return (ClassicDesktopStyleApplicationLifetime) ApplicationLifetime!;
    }

    public void Shutdown()
    {
        Lifetime().Shutdown();
        Lifetime().Dispose();
    }

    public MainWindow MainWindow()
    {
        return (MainWindow) Lifetime().MainWindow;
    }
}