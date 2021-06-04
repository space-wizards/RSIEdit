using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class TextInputWindowViewModel : ViewModelBase
    {
        public TextInputWindowViewModel(string title, string header)
        {
            Header = header;
        }

        public Interaction<string, Unit> ConfirmAction { get; } = new();

        public Interaction<Unit, Unit> DeclineAction { get; } = new();

        public string Title { get; }

        public string Header { get; }

        public string SubmittedText { get; set; } = string.Empty;

        public async void Confirm()
        {
            await ConfirmAction.Handle(SubmittedText ?? string.Empty);
        }

        public async void Decline()
        {
            await DeclineAction.Handle(Unit.Default);
        }
    }
}
