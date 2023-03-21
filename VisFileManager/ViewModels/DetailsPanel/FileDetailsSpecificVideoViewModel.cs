using System.Threading.Tasks;
using VisFileManager.Helpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class FileDetailsSpecificVideoViewModel : FileDetailsSpecificBaseViewModel, IFileDetailsSpecificVideoViewModel
    {
        public FileDetailsSpecificVideoViewModel(FormattedPath formattedPath, FileInfoHelper fileInfoHelper) : base(formattedPath, fileInfoHelper)
        {
        }

        public uint? VideoWidth => _fileInfoHelper.GetVideoWidth(_formattedPath);

        public uint? VideoHeigh => _fileInfoHelper.GetVideoHeight(_formattedPath);
        
        public string FrameRate => _fileInfoHelper.GetVideoFrameratePerSecondFormatted(_formattedPath); 
        
        public string Bitrate=> _fileInfoHelper.GetVideoBitratePerSecondFormatted(_formattedPath);
        
        public string Duration=> _fileInfoHelper.GetMediaDurationFormatted(_formattedPath); 
        
    }
}
