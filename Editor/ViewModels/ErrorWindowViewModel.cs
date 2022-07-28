namespace Editor.ViewModels;

public class ErrorWindowViewModel : ViewModelBase
{
    public ErrorWindowViewModel(string error)
    {
        Error = error;
    }

    public string Error { get; }
}