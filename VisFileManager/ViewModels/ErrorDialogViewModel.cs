using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    [Export(typeof(IErrorDialogViewModel))]
    public class ErrorDialogViewModel : ViewModelBase, IErrorDialogViewModel
    {
        private readonly IMessenger _messenger;
        private bool _isVisible;
        private string _message;
        private ICommand _okCommand;
        private string _commandCaption;

        [ImportingConstructor]
        public ErrorDialogViewModel(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            private set
            {
                _isVisible = value;
                _messenger.Send(MessageIds.BlurBackgroundMessage, new BlurBackgroundMessage(value, false));
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            private set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return _okCommand;
            }
            private set
            {
                _okCommand = value;
                OnPropertyChanged();
            }
        }

        public string CommandCaption
        {
            get { return _commandCaption; }
            private set
            {
                _commandCaption = value;
                OnPropertyChanged();
            }
        }

        private void ShowError(string message, ICommand okCommand, string commandCaption)
        {
            Message = message;
            IsVisible = true;
            OkCommand = okCommand;
            CommandCaption = commandCaption;
        }

        public void ShowError(string message, Action action)
        {
            ShowError(message, new RelayCommand(() => { IsVisible = false; action.Invoke(); }), "OK");
        }
    }
}
