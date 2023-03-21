using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    [Export(typeof(IThreeButtonsDialogViewModel))]
    public class ThreeButtonsDialogViewModel : ViewModelBase, IThreeButtonsDialogViewModel
    {
        private readonly IMessenger _messenger;
        private bool _isVisible = false;
        private bool _secondCommandVisible;
        private string _message;
        private ICommand _cancelCommand;
        private ICommand _firstCommand;
        private ICommand _secondCommand;
        private string _firstCommandCaption;
        private string _secondCommandCaption;
        private string _cancelCaption = "Cancel";


        [ImportingConstructor]
        public ThreeButtonsDialogViewModel(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public bool IsVisible
        {
            get { return _isVisible; }
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

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand;
            }
            private set
            {
                _cancelCommand = value;
                OnPropertyChanged();
            }
        }

        public string CancelCaption
        {
            get { return _cancelCaption; }
            private set
            {
                _cancelCaption = value;
                OnPropertyChanged();
            }
        }

        public ICommand FirstCommand
        {
            get
            {
                return _firstCommand;
            }
            private set
            {
                _firstCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand SecondCommand
        {
            get
            {
                return _secondCommand;
            }
            private set
            {
                _secondCommand = value;
                OnPropertyChanged();
            }
        }

        public string FirstCommandCaption
        {
            get { return _firstCommandCaption; }
            private set
            {
                _firstCommandCaption = value;
                OnPropertyChanged();
            }
        }

        public string SecondCommandCaption
        {
            get { return _secondCommandCaption; }
            private set
            {
                _secondCommandCaption = value;
                OnPropertyChanged();
            }
        }

        public bool SecondCommandVisible
        {
            get { return _secondCommandVisible; }
            private set
            {
                _secondCommandVisible = value;
                OnPropertyChanged();
            }
        }

        public void ShowDialog(string message, Action firstCommand, string firstCommandCaption, Action secondCommand, string secondCommandCaption, CancellationTokenSource cancellationTokenSource)
        {
            Message = message;
            FirstCommandCaption = firstCommandCaption;
            SecondCommandCaption = secondCommandCaption;

            CancelCommand = new RelayCommand(() => {cancellationTokenSource.Cancel(); IsVisible = false; });
            FirstCommand = new RelayCommand(() => {  firstCommand.Invoke(); IsVisible = false; });
            if (!string.IsNullOrEmpty(secondCommandCaption))
            {
                SecondCommand = new RelayCommand(() => { secondCommand.Invoke(); IsVisible = false; });
                SecondCommandVisible = true;
            }
            else
            {
                SecondCommandVisible = false;
            }

            IsVisible = true;
        }

        public void ShowDialog(string message, Action firstCommand, string firstCommandCaption, CancellationTokenSource cancellationTokenSource)
        {
            Message = message;
            FirstCommandCaption = firstCommandCaption;

            CancelCommand = new RelayCommand(() => { cancellationTokenSource.Cancel(); IsVisible = false; });
            FirstCommand = new RelayCommand(() => { firstCommand.Invoke(); IsVisible = false; });
            SecondCommand = null;

            SecondCommandVisible = false;
            IsVisible = true;
        }
    }
}
