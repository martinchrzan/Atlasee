using System.IO;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;
using static VisFileManager.Validators.PathValidator;

namespace VisFileManager.ViewModels
{
    public class DirectoryItemViewModel : ViewModelBase, IDirectoryItemViewModel
    {
        private readonly IGlobalFileManager _globalFileManager;
        private readonly DriveInfo _driveInfo;
        private FormattedPath _formattedPath;
        private readonly IMessenger _messenger;
        private bool _isDrive = false;

        public DirectoryItemViewModel(IGlobalFileManager globalFileManager, FormattedPath formattedPath, IMessenger messenger)
        {
            _formattedPath = formattedPath;
            _messenger = messenger;
            _globalFileManager = globalFileManager;

            Name = formattedPath.Name;
            PathType = formattedPath.PathType;

            SelectedCommand = new RelayCommand(SelectDirectory);            
        }

        public DirectoryItemViewModel(IGlobalFileManager globalFileManager, DriveInfo driveInfo, IMessenger messenger) : this(globalFileManager, FormattedPath.CreateFormattedPath(driveInfo), messenger)
        {
            _globalFileManager = globalFileManager;
            _driveInfo = driveInfo;
            _isDrive = true;
        }


        public string Name { get; }

        public PathType PathType { get; }

        public ICommand SelectedCommand { get; }

        public ICommand PropertiesCommand
        {
            get
            {
                return _isDrive ? new RelayCommand(OpenDriveProperties) : new RelayCommand(OpenDirectoryProperties);
            }
        }

        private void SelectDirectory()
        {
            _globalFileManager.SetCurrentPath(_formattedPath);
        }

        private void OpenDirectoryProperties() => _messenger.Send(MessageIds.OpenFileDetailsMessage, new OpenFileDetailsMessage(_formattedPath, false));

        private void OpenDriveProperties() => _messenger.Send(MessageIds.OpenDriveDetailsMessage, _driveInfo);
    }
}
