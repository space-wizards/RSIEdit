using System;
using System.Windows.Input;

namespace Editor.Views.RsiItemCommands
{
    public class RedoCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not MainWindow {ViewModel: {Rsi: { }}} view)
            {
                return;
            }

            var vm = view.ViewModel.Rsi;

            if (view.ViewModel.Rsi.TryRedoDelete(out var index))
            {
                vm.ReselectState(index);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
