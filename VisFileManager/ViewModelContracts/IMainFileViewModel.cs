using System;
using VisFileManager.Common;

namespace VisFileManager.ViewModelContracts
{
    public interface IMainFileViewModel
    {
        RangeObservableCollection<IFileAndDriveItemViewModelBase> FileItemViewModels { get; }
        
        bool IsMyComputerView { get; }

        event EventHandler<int> NumberOfItemsChanged;

        bool SearchInProgress { get; }

        bool LoadingItems { get; }
    }
}
