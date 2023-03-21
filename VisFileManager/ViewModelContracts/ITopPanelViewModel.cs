using System.Collections.ObjectModel;
using System.Windows.Input;
using VisFileManager.Enums;
using VisFileManager.Validators;

namespace VisFileManager.ViewModelContracts
{
    public interface ITopPanelViewModel
    {
        ICommand ParentDirectoryCommand { get; }

        ICommand BackDirectoryCommand { get; }

        ICommand ForwardDirectoryCommand { get; }

        FormattedPath ParentDirectoryPath { get; }

        FormattedPath BackDirectoryPath { get; }

        FormattedPath ForwardDirectoryPath { get; }

        ICommand UseGridView { get; }

        ICommand UseListView { get; }

        ICommand PasteCommand { get; }

        ObservableCollection<string> PasteItems { get; }
        
        SortingDirection SortingDirection { get; }

        SortingField SortingField { get; }

        ICreateNewItemViewModel CreateNewItemViewModel { get; }

        bool CreateNewItemViewModelEnabled { get; }

        ISearchViewModel SearchViewModel { get; }

        ICommand OpenGalleryCommand { get; }
    }
}
