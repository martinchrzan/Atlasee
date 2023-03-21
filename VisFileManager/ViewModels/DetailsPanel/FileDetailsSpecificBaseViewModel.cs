using VisFileManager.Helpers;
using VisFileManager.Validators;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public abstract class FileDetailsSpecificBaseViewModel
    {
        protected readonly FormattedPath _formattedPath;
        protected readonly FileInfoHelper _fileInfoHelper;

        public FileDetailsSpecificBaseViewModel(FormattedPath formattedPath, FileInfoHelper fileInfoHelper)
        {
            _formattedPath = formattedPath;
            _fileInfoHelper = fileInfoHelper;
        }
    }
}
