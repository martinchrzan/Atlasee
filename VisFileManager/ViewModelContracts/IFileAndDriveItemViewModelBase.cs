using System.Windows.Input;
using System.Windows.Media.Imaging;
using VisFileManager.Validators;

namespace VisFileManager.ViewModelContracts
{
    public interface IFileAndDriveItemViewModelBase
    {
        string Name { get; }

        FormattedPath FullFormattedPath { get; }
        
        string Type { get; }

        string SizeInKB { get; }

        ICommand OpenCommand { get; }

        ICommand OpenPropertiesCommand { get; }

        string CachedFullPath { get; }
    }
}
