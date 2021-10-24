using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Editor.Views;
using NUnit.Framework;

namespace Test
{
    public class AvaloniaTest
    {
        private static TestApp? _app;

        protected static TestApp App => _app ?? throw new NullReferenceException();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var tcs = new TaskCompletionSource<SynchronizationContext>();

            var app = AppBuilder
                .Configure<TestApp>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .AfterSetup(builder =>
                {
                    _app = (TestApp) builder.Instance;
                    tcs.SetResult(SynchronizationContext.Current!);
                })
                .UseHeadless();

            var thread = new Thread(() => app.StartWithClassicDesktopLifetime(Array.Empty<string>()))
            {
                IsBackground = true
            };

            thread.Start();

            SynchronizationContext.SetSynchronizationContext(tcs.Task.Result);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await Post(() =>
            {
                _app?.Shutdown();
            });
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_app == null)
            {
                return;
            }

            await Post(() =>
            {
                var lifetime = _app.Lifetime();
                foreach (var window in lifetime.Windows)
                {
                    if (window is MainWindow)
                    {
                        continue;
                    }
                    
                    window.Close();
                }
                
                _app.MainWindow().ViewModel!.Reset();
            });
        }

        protected async Task Post(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            await Dispatcher.UIThread.InvokeAsync(action, priority);
        }
    }
}
