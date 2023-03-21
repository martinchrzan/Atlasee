using System.Collections.ObjectModel;
using System.IO;

namespace VisFileManager.ViewModelContracts
{
    public interface ISidePanelViewModel
    {
        ObservableCollection<IDirectoryItemViewModel> DirectoryItemViewModels { get; }

        string CurrentPath { get; }

        int ItemsInCurrentPath { get; }

        ObservableCollection<IDirectoryItemViewModel> Drives { get; }
    }
}
