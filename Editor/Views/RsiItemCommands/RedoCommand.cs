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
            (parameter as MainWindow)?.ViewModel?.Rsi?.TryRedoDelete();
        }

        public event EventHandler? CanExecuteChanged;
    }
}
