using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Editor.ViewModels;
using Editor.Views.RsiItemCommands;

namespace Editor.Views
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class RsiItemView : ReactiveUserControl<RsiItemViewModel>
    {
        private readonly DeleteStateCommand _deleteStateCommand = new();
        private readonly UndoCommand _undoCommand = new();
        private readonly RedoCommand _redoCommand = new();

        public RsiItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var deleteGesture = new KeyGesture(Key.Delete);
            var deleteBinding = new KeyBinding
            {
                Command = _deleteStateCommand,
                CommandParameter = this,
                Gesture = deleteGesture
            };

            var undoGesture = new KeyGesture(Key.Z, KeyModifiers.Control);
            var undoBinding = new KeyBinding
            {
                Command = _undoCommand,
                CommandParameter = this,
                Gesture = undoGesture
            };

            var redoGesture = new KeyGesture(Key.Y, KeyModifiers.Control);
            var redoBinding = new KeyBinding
            {
                Command = _redoCommand,
                CommandParameter = this,
                Gesture = redoGesture
            };

            var redoGestureAlternative = new KeyGesture(Key.Z, KeyModifiers.Control | KeyModifiers.Shift);
            var redoBindingAlternative = new KeyBinding
            {
                Command = _redoCommand,
                CommandParameter = this,
                Gesture = redoGestureAlternative
            };

            KeyBindings.AddRange(new[]
            {
                deleteBinding,
                undoBinding,
                redoBinding,
                redoBindingAlternative
            });
        }
    }
}
