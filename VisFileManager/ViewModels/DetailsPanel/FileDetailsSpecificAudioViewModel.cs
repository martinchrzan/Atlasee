using VisFileManager.Helpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class FileDetailsSpecificAudioViewModel : FileDetailsSpecificBaseViewModel, IFileDetailsSpecificAudioViewModel
    {
        public FileDetailsSpecificAudioViewModel(FormattedPath formattedPath, FileInfoHelper fileInfoHelper) : base(formattedPath, fileInfoHelper)
        {
        }

        public string Bitrate => _fileInfoHelper.GetAudioBitratePerSecondFormatted(_formattedPath);

        public string Duration => _fileInfoHelper.GetMediaDurationFormatted(_formattedPath);
    }
}
