using System;
using System.Windows.Input;

namespace Editor.Views.RsiItemCommands
{
    public class UndoCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            (parameter as MainWindow)?.ViewModel?.Rsi?.TryRestore();
        }

        public event EventHandler? CanExecuteChanged;
    }
}
