using System.ComponentModel.Composition;
using VisFileManager.Helpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;
using VisFileManager.ViewModels.DetailsPanel;

namespace VisFileManager.ViewModelFactories.DetailsPanel
{
    [Export(typeof(ISpecificFileDetailsViewModelFactory))]
    public class SpecificFileDetailsViewModelFactory : ISpecificFileDetailsViewModelFactory
    {
        private readonly FileInfoHelper _fileInfoHelper;

        [ImportingConstructor]
        public SpecificFileDetailsViewModelFactory(FileInfoHelper fileInfoHelper)
        {
            _fileInfoHelper = fileInfoHelper;
        }

        public ISpecificFileDetailsViewModel CreateSpecifiFileDetailsViewModel(FormattedPath path)
        {
            switch(_fileInfoHelper.GetGeneralFileType(path))
            {
                case FileInfoHelper.PerceivedTypeEnum.Image:
                    return new FileDetailsSpecificImageViewModel(path, _fileInfoHelper);
                case FileInfoHelper.PerceivedTypeEnum.Video:
                    return new FileDetailsSpecificVideoViewModel(path, _fileInfoHelper);
                case FileInfoHelper.PerceivedTypeEnum.Audio:
                    return new FileDetailsSpecificAudioViewModel(path, _fileInfoHelper);
                default:
                    return null;
            }
        }
    }
}
