using System;
using System.Windows.Input;

namespace Editor.Views.Commands
{
    public class UndoCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not MainWindow {ViewModel: {CurrentOpenRsi: { }}} window)
            {
                return;
            }

            var rsi = window.ViewModel.CurrentOpenRsi;

            if (rsi.TryRestore(out var restored))
            {
                rsi.SelectedState = restored;
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
