using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    [Export(typeof(ICopyProgressBottomPanelViewModel))]
    public class CopyProgressBottomPanelViewModel : ViewModelBase, ICopyProgressBottomPanelViewModel
    {
        private bool _multipleFiles;
        private bool _visible;
        private bool _smallSize;
        private string _currentTransferSpeed;
        private double _overallProgressPercentage;
        private double _partialProgressPercentage;
        private string _overallProgresCurrentFile;
        private string _partialProgressCurrentFile;
        private readonly IDispatcherTaskScheduler _dispatcherTaskScheduler;
        private readonly IMessenger _messenger;
        private ICommand _cancelCommand;

        [ImportingConstructor]
        public CopyProgressBottomPanelViewModel(IDispatcherTaskScheduler dispatcherTaskScheduler, IMessenger messenger)
        {
            _dispatcherTaskScheduler = dispatcherTaskScheduler;
            _messenger = messenger;

        }

        public bool MultipleFiles
        {
            get
            {
                return _multipleFiles;
            }
            set
            {
                _multipleFiles = value;
                OnPropertyChanged();
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                if(value)
                {
                    _messenger.Send(MessageIds.BlurBackgroundMessage, new BlurBackgroundMessage(true, false));   
                }
                OnPropertyChanged();
            }
        }

        public bool SmallSize
        {
            get { return _smallSize; }
            set
            {
                _smallSize = value;
                OnPropertyChanged();
            }
        }

        public double OverallProgressPercentage
        {
            get { return _overallProgressPercentage; }
            set
            {
                _overallProgressPercentage = value;
                OnPropertyChanged();
            }
        }

        public double PartialProgressPercentage
        {
            get { return _partialProgressPercentage; }
            set
            {
                _partialProgressPercentage = value;
                OnPropertyChanged();
            }
        }

        public string CurrentTransferSpeed
        {
            get { return _currentTransferSpeed; }
            set
            {
                _currentTransferSpeed = value;
                OnPropertyChanged();
            }
        }
        public string OverallProgressCurrentFile
        {
            get { return _overallProgresCurrentFile; }
            set
            {
                _overallProgresCurrentFile = value;
                OnPropertyChanged();
            }
        }

        public string PartialProgressCurrentFile
        {
            get { return _partialProgressCurrentFile; }
            set
            {
                _partialProgressCurrentFile = value;
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

        public void Finished()
        {
            PartialProgressCurrentFile = string.Empty;
            PartialProgressPercentage = 100;
            OverallProgressPercentage = 100;
            CurrentTransferSpeed = string.Empty;
            _dispatcherTaskScheduler.ScheduleAction(() => {
                Visible = false;
                _messenger.Send(MessageIds.BlurBackgroundMessage, new BlurBackgroundMessage(false, false));
            }, 1000);
        }

        public void Prepare(bool multipleFiles, CancellationTokenSource cancellationTokenSource)
        {
            MultipleFiles = multipleFiles;
            PartialProgressCurrentFile = string.Empty;
            OverallProgressCurrentFile = string.Empty;
            PartialProgressPercentage = 0;
            OverallProgressPercentage = 0;
            CurrentTransferSpeed = string.Empty;
            CancelCommand = new RelayCommand(() => cancellationTokenSource.Cancel());
        }
    }
}
