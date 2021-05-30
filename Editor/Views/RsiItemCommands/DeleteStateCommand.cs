using System;
using System.Windows.Input;

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
            if (parameter is not MainWindow {ViewModel: {Rsi: {SelectedState: { }}}} view)
            {
                return;
            }

            var vm = view.ViewModel.Rsi;

            if (vm.TryDelete(vm.SelectedState, out var index))
            {
                vm.ReselectState(index);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
