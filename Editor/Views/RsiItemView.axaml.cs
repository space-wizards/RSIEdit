using System;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class RsiItemView : ReactiveUserControl<RsiItemViewModel>
    {
        private readonly DeleteStateCommand _deleteStateCommand = new();

        public RsiItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var gesture = new KeyGesture(Key.Delete);
            var binding = new KeyBinding
            {
                Command = _deleteStateCommand,
                CommandParameter = this,
                Gesture = gesture
            };

            KeyBindings.Add(binding);
        }

        private class DeleteStateCommand : ICommand
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
                var index = vm.Delete(vm.SelectedState);

                if (vm.States.Count > index)
                {
                    vm.SelectedState = vm.States[index];
                    FocusManager.Instance.Focus(view);
                }
            }

            public event EventHandler? CanExecuteChanged;
        }
    }
}
