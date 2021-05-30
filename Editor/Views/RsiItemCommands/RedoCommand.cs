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
            if (parameter is not RsiItemView {ViewModel: {SelectedState: { }}} view)
            {
                return;
            }

            view.ViewModel.TryRedoDelete();
        }

        public event EventHandler? CanExecuteChanged;
    }
}
