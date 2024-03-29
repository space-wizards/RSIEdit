﻿using System.IO;
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

public partial class PreferencesWindow : ReactiveWindow<PreferencesWindowViewModel>
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

    private void Save(InteractionContext<Preferences, Unit> arg)
    {
        var filePath = "preferences.json";
        using var metaJsonFile = File.Create(filePath);
        var preferences = arg.Input;

        JsonSerializer.Serialize(metaJsonFile, preferences, PreferencesJsonContext.Default.Preferences);

        Close(preferences);
        arg.SetOutput(Unit.Default);
    }

    private void Cancel(InteractionContext<Preferences, Unit> arg)
    {
        Close();
        arg.SetOutput(Unit.Default);
    }
}