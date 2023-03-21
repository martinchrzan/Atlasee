using VisFileManager.Helpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class FileDetailsSpecificImageViewModel : FileDetailsSpecificBaseViewModel, IFileDetailsSpecificImageViewModel
    {
        public FileDetailsSpecificImageViewModel(FormattedPath formattedPath, FileInfoHelper fileInfoHelper) : base(formattedPath, fileInfoHelper)
        {
        }

        public int ImageWidth => _fileInfoHelper.GetImageWidth(_formattedPath);

        public int ImageHeight => _fileInfoHelper.GetImageHeight(_formattedPath);

        public uint? ImageBitness => _fileInfoHelper.GetImageBitDepth(_formattedPath);
    }
}
