using System;
using System.Windows.Input;

namespace VisFileManager.ViewModelContracts
{
    public interface IErrorDialogViewModel
    {
        bool IsVisible { get; }

        string Message { get; }

        ICommand OkCommand { get; }

        string CommandCaption { get; }

        void ShowError(string message, Action command);
    }
}
