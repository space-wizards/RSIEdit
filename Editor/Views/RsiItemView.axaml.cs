using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using Editor.Views.Events;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Editor.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class RsiItemView : ReactiveUserControl<RsiItemViewModel>
{
    public RsiItemView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            d.Add(this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(new AnonymousObserver<RsiItemViewModel?>(vm =>
                {
                    if (vm != null)
                    {
                        d.Add(vm.ImportImageInteraction.RegisterHandler(ImportImage));
                        d.Add(vm.ExportPngInteraction.RegisterHandler(ExportPng));
                        d.Add(vm.ErrorDialog.RegisterHandler(ShowError));
                        d.Add(vm.CloseInteraction.RegisterHandler(Close));
                        d.Add(vm.States.Subscribe(new AnonymousObserver<RsiStateViewModel>(s => d.Add(s.Image.Preview))));
                        d.Add(vm);
                    }
                })));
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ShowError(InteractionContext<ErrorWindowViewModel, Unit> interaction)
    {
        var args = new ShowErrorEvent(interaction.Input) {RoutedEvent = MainWindow.ShowErrorEvent};
        RaiseEvent(args);

        interaction.SetOutput(Unit.Default);
    }

    private void ImportImage(InteractionContext<Unit, string> interaction)
    {
        var dialog = new OpenFileDialog
        {
            AllowMultiple = false,
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = "Image Files",
                    Extensions = RsiItemViewModel.ValidExtensions,
                }
            }
        };
        var args = new OpenFileDialogEvent(dialog) {RoutedEvent = MainWindow.OpenFileEvent};
        RaiseEvent(args);

        interaction.SetOutput(args.Files.FirstOrDefault() ?? string.Empty);
    }

    private void ExportPng(InteractionContext<Image<Rgba32>, Unit> interaction)
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            DefaultExtension = "png",
            InitialFileName = ViewModel?.SelectedStates[0].Image.State.Name ?? string.Empty,
        };

        var args = new SaveFileDialogEvent(dialog, interaction.Input) { RoutedEvent = MainWindow.SaveFileEvent };
        RaiseEvent(args);

        interaction.SetOutput(Unit.Default);
    }

    private void Close(InteractionContext<RsiItemViewModel, Unit> interaction)
    {
        var args = new CloseRsiEvent(interaction.Input) {RoutedEvent = MainWindow.CloseRsiEvent};
        RaiseEvent(args);

        interaction.SetOutput(Unit.Default);
    }
}
