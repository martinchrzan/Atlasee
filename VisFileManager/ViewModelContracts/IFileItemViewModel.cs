using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace VisFileManager.ViewModelContracts
{
    public interface IFileItemViewModel : IFileAndDriveItemViewModelBase
    {
        bool IsDirectory { get; }

        DateTime Modified { get; }

        ImageSource Icon { get; }

        ImageSource IconBig { get; }

        ulong? TotalSizeInBytes { get; }

        bool IsSearchResult { get; }

        ICommand RemoveCommand { get;  }

        void Initialize();
    }
}
