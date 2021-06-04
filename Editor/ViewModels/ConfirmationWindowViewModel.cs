using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class ConfirmationWindowViewModel : ViewModelBase
    {
        private string _text;

        public ConfirmationWindowViewModel(string text)
        {
            _text = text;
        }

        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public Interaction<Unit, Unit> ConfirmAction { get; } = new();

        public Interaction<Unit, Unit> DeclineAction { get; } = new();

        public async void Confirm()
        {
            await ConfirmAction.Handle(Unit.Default);
        }

        public async void Decline()
        {
            await DeclineAction.Handle(Unit.Default);
        }
    }
}
