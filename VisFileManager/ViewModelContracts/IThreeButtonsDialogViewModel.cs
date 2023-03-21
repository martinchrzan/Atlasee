using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VisFileManager.ViewModelContracts
{
    public interface IThreeButtonsDialogViewModel
    {
        bool IsVisible { get; }

        string Message { get; }

        ICommand CancelCommand { get; }

        string CancelCaption { get; }

        ICommand FirstCommand { get;  }

        ICommand SecondCommand { get; }

        bool SecondCommandVisible { get; }

        string FirstCommandCaption { get; }
    
        string SecondCommandCaption { get;  }

        void ShowDialog(string message, Action firstCommand, string firstCommandCaption, CancellationTokenSource cancellationTokenSource);

        void ShowDialog(string message, Action firstCommand, string firstCommandCaption, Action secondCommand, string secondCommandCaption, CancellationTokenSource cancellationTokenSource);
    }
}
