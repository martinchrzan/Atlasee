using System.IO;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModelFactories.DetailsPanel
{
    public interface IFileAndDirectoryDetailsViewModelFactory
    {
        IDetailsViewModelBase CreateFileOrDirectoryDetailsViewModel(FormattedPath path);

        IDriveDetailsViewModel CreateDriveDetailsViewModel(DriveInfo driveInfo);
    }
}
