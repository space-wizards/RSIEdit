using System;
using System.Windows.Input;

namespace Editor.Views.Commands
{
    public class NewCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not MainWindow {ViewModel: { }} window)
            {
                return;
            }

            window.DoNewRsi();
        }

        public event EventHandler? CanExecuteChanged;
    }
}
