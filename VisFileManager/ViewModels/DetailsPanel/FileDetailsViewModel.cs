using System;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;
using VisFileManager.ViewModelFactories.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class FileDetailsViewModel : DetailsViewModelBase, IFileDetailsViewModel
    {
        private const string InvalidDate = "Invalid date";
        private readonly FormattedPath _path;
        private readonly FileInfoHelper _fileInfoHelper;

        public FileDetailsViewModel(FormattedPath path, FileInfoHelper fileInfoHelper, ISpecificFileDetailsViewModelFactory specificFileDetailsViewModelFactory, IFileOperationsManager fileOperationsManager ) : base(path, fileOperationsManager)
        {
            _path = path;
            _fileInfoHelper = fileInfoHelper;
            SpecificFileDetailsViewModel = specificFileDetailsViewModelFactory.CreateSpecifiFileDetailsViewModel(_path);
        }

        public string FileSize
        {
            get
            {
                return _fileInfoHelper.GetFileSizeFormatted(_path);
            }
        }

        public string FileSizeOnDisk
        {
            get
            {
                return _fileInfoHelper.GetFileSizeOnDiskFormatted(_path);
            }
        }


        public string TypeOfFile
        {
            get
            {
                return _fileInfoHelper.GetSpecificFileType(_path);
            }
        }

        public string Created
        {
            get
            {
                var created = _fileInfoHelper.GetCreateDate(_path);
                if(created == null)
                {
                    return InvalidDate;
                }
                return created.ToString();
            }
        }

        public string Modified
        {
            get
            {
                var created = _fileInfoHelper.GetModifiedDate(_path);
                if (created == null)
                {
                    return InvalidDate;
                }
                return created.ToString();
            }
        }

        public string Accessed
        {
            get
            {
                var created = _fileInfoHelper.GetAccessedDate(_path);
                if (created == null)
                {
                    return InvalidDate;
                }
                return created.ToString();
            }
        }

        public ISpecificFileDetailsViewModel SpecificFileDetailsViewModel { get; }
    }
}
