using System;
using System.Windows.Input;

namespace Editor.Views.Commands
{
    public class DeleteStateCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not MainWindow {ViewModel: {CurrentOpenRsi: {SelectedState: { }}}} view)
            {
                return;
            }

            var vm = view.ViewModel.CurrentOpenRsi;

            if (vm.TryDelete(vm.SelectedState, out var index))
            {
                vm.ReselectState(index);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
