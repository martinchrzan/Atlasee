using System.IO;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModelFactories
{
    public interface IDirectoryItemViewModelFactory
    {
        IDirectoryItemViewModel CreateDirectoryItemViewModel(IGlobalFileManager globalFileManager, FormattedPath formattedPath);

        IDirectoryItemViewModel CreateDriveItemViewModel(IGlobalFileManager globalFileManager, DriveInfo driveInfo);
    }
}
