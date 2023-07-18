using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Editor.ViewModels;
using Editor.Views;
using NUnit.Framework;

namespace Test;

public class AvaloniaTest
{
    protected static TestApp App => GlobalSetup.App ?? throw new NullReferenceException();

    protected static MainWindow? Window => App.MainWindow();

    protected static MainWindowViewModel Vm => Window?.ViewModel ?? throw new NullReferenceException();

    [TearDown]
    public async Task TearDown()
    {
        if (GlobalSetup.App == null)
        {
            return;
        }

        await Post(() =>
        {
            var lifetime = App.Lifetime();
            foreach (var window in lifetime.Windows)
            {
                if (window is MainWindow)
                {
                    continue;
                }
                    
                window.Close();
            }
                
            App.MainWindow()?.ViewModel!.Reset();
        });
    }

    public static async Task Post(Action action, DispatcherPriority? priority = null)
    {
        await Dispatcher.UIThread.InvokeAsync(action, priority ?? DispatcherPriority.Normal);
    }

    public static async Task Post(Func<Task> action, DispatcherPriority? priority)
    {
        await Dispatcher.UIThread.InvokeAsync(action, priority ?? DispatcherPriority.Normal);
    }
}