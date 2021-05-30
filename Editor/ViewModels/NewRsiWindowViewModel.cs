using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace Editor.ViewModels
{
    public class NewRsiWindowViewModel : ViewModelBase
    {
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
