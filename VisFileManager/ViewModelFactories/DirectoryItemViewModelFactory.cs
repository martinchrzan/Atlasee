using System.ComponentModel.Composition;
using System.IO;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Messenger;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;
using VisFileManager.ViewModels;

namespace VisFileManager.ViewModelFactories
{
    [Export(typeof(IDirectoryItemViewModelFactory))]
    public class DirectoryItemViewModelFactory : IDirectoryItemViewModelFactory
    {
        private readonly IMessenger _messenger;

        [ImportingConstructor]
        public DirectoryItemViewModelFactory(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public IDirectoryItemViewModel CreateDirectoryItemViewModel(IGlobalFileManager globalFileManager, FormattedPath formattedPath)
        {
            return new DirectoryItemViewModel(globalFileManager, formattedPath, _messenger);
        }

        public IDirectoryItemViewModel CreateDriveItemViewModel(IGlobalFileManager globalFileManager, DriveInfo driveInfo)
        {
            return new DirectoryItemViewModel(globalFileManager, driveInfo, _messenger);
        }
    }
}
