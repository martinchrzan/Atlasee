using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;
using VisFileManager.ViewModels.DetailsPanel;

namespace VisFileManager.ViewModelFactories.DetailsPanel
{
    [Export(typeof(IFileAndDirectoryDetailsViewModelFactory))]
    public class FileAndDirectoryDetailsViewModelFactory : IFileAndDirectoryDetailsViewModelFactory
    {
        private readonly FileInfoHelper _helper;
        private readonly ISpecificFileDetailsViewModelFactory _specificFileDetailsViewModelFactory;
        private readonly IFileOperationsManager _fileOperationsManager;

        [ImportingConstructor]
        public FileAndDirectoryDetailsViewModelFactory(FileInfoHelper helper, ISpecificFileDetailsViewModelFactory specificFileDetailsViewModelFactory, IFileOperationsManager fileOperationsManager)
        {
            _helper = helper;
            _specificFileDetailsViewModelFactory = specificFileDetailsViewModelFactory;
            _fileOperationsManager = fileOperationsManager;
        }

        public IDriveDetailsViewModel CreateDriveDetailsViewModel(DriveInfo driveInfo)
        {
            return new DriveDetailsViewModel(driveInfo);
        }

        public IDetailsViewModelBase CreateFileOrDirectoryDetailsViewModel(FormattedPath path)
        {
            if(path.PathType == PathValidator.PathType.Directory)
            {
                return new DirectoryDetailsViewModel(path, _helper, _fileOperationsManager);
            }
            else if(path.PathType == PathValidator.PathType.File)
            {
                return new FileDetailsViewModel(path, _helper, _specificFileDetailsViewModelFactory, _fileOperationsManager);
            }
            return null;
        }
    }
}
