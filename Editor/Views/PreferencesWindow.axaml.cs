using System.IO;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.Models;
using Editor.ViewModels;
using ReactiveUI;

namespace Editor.Views;

public class PreferencesWindow : ReactiveWindow<PreferencesWindowViewModel>
{
    public PreferencesWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        this.WhenActivated(d =>
        {
            var vm = ViewModel!;

            d.Add(vm.SaveAction.RegisterHandler(Save));
            d.Add(vm.CancelAction.RegisterHandler(Cancel));
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task Save(InteractionContext<Preferences, Unit> arg)
    {
        var filePath = "preferences.json";
        await File.WriteAllTextAsync(filePath, string.Empty);

        var metaJsonFile = File.OpenWrite(filePath);
        var preferences = arg.Input;

        await JsonSerializer.SerializeAsync(metaJsonFile, preferences);
        await metaJsonFile.FlushAsync();
        await metaJsonFile.DisposeAsync();

        Close(preferences);
        arg.SetOutput(Unit.Default);
    }

    private void Cancel(InteractionContext<Preferences, Unit> arg)
    {
        Close();
        arg.SetOutput(Unit.Default);
    }
}