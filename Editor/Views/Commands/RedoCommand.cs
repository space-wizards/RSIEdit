using System;
using System.Windows.Input;

namespace Editor.Views.Commands
{
    public class RedoCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not MainWindow {ViewModel: {CurrentOpenRsi: { }}} view)
            {
                return;
            }

            var vm = view.ViewModel.CurrentOpenRsi;

            if (view.ViewModel.CurrentOpenRsi.TryRedoDelete(out var index))
            {
                vm.ReselectState(index);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
