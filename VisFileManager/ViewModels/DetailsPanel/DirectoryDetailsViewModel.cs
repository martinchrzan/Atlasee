using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;
using static VisFileManager.Helpers.FileInfoHelper;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class DirectoryDetailsViewModel : DetailsViewModelBase, IDirectoryDetailsViewModel
    {
        private const string InvalidDate = "Invalid date";

        private DirectorySizeInfo _directorySizeInfo;
        private readonly FormattedPath _formattedPath;
        private readonly FileInfoHelper _fileInfoHelper;
        private readonly object _loadDirectoryInfoLock = new object();

        public DirectoryDetailsViewModel(FormattedPath formattedPath, FileInfoHelper fileInfoHelper, IFileOperationsManager fileOperationsManager) : base(formattedPath, fileOperationsManager)
        {
            _formattedPath = formattedPath;
            _fileInfoHelper = fileInfoHelper;
        }

        public string DirectorySize
        {
            get
            {
                LoadDirectoryInfo();

                return ToSizeWithSuffix(_directorySizeInfo.Size); 
            }
        }

        public string Contains
        {
            get
            {
                LoadDirectoryInfo();
                return string.Format("{0:N0} Files, {1:N0} Folders", _directorySizeInfo.FileCount, _directorySizeInfo.DirectoryCount);
            }
        }

        public string Created
        {
            get
            {
                var created = _fileInfoHelper.GetCreateDate(_formattedPath);
                if (created == null)
                {
                    return InvalidDate;
                }
                return created.ToString();
            }
        }


        private void LoadDirectoryInfo()
        {
            lock(_loadDirectoryInfoLock)
            {
                if(_directorySizeInfo == null)
                {
                    _directorySizeInfo = _fileInfoHelper.GetDirectoryInfo(_formattedPath);
                }
            }
        }
    }
}
