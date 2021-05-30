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

            int? nextSelectedState = null;

            // Select either the next or previous one
            if (vm.States.Count > index)
            {
                nextSelectedState = index;
            }
            else if (vm.States.Count == index && vm.States.Count > 0)
            {
                nextSelectedState = index - 1;
            }

            if (nextSelectedState != null)
            {
                vm.SelectedState = vm.States[nextSelectedState.Value];
                FocusManager.Instance.Focus(view);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
