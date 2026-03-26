using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace Editor.ViewModels;

public class TextReplaceWindowViewModel : ViewModelBase
{
    public TextReplaceWindowViewModel(string title)
    {
        Title = title;
    }

    public Interaction<(string Replace, string With), Unit> ConfirmAction { get; } = new();

    public Interaction<Unit, Unit> DeclineAction { get; } = new();

    public string Title { get; }

    public string Replace { get; set; } = string.Empty;

    public string With { get; set; } = string.Empty;

    public async void Confirm()
    {
        await ConfirmAction.Handle((Replace, With));
    }

    public async void Decline()
    {
        await DeclineAction.Handle(Unit.Default);
    }
}
