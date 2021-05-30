using System;
using System.Windows.Input;
using Avalonia.Input;

namespace Editor.Views.RsiItemCommands
{
    public class DeleteStateCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not RsiItemView {ViewModel: {SelectedState: { }}} view)
            {
                return;
            }

            var vm = view.ViewModel;

            if (!vm.TryDelete(vm.SelectedState, out var index))
            {
                return;
            }

            if (vm.States.Count > index)
            {
                vm.SelectedState = vm.States[index];
                FocusManager.Instance.Focus(view);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
