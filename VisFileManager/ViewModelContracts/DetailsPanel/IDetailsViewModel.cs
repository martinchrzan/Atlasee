using System.Collections.ObjectModel;
using System.Windows.Input;
using VisFileManager.Controls;

namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IDetailsViewModel
    {
        bool IsOpened { get; set; }

        ICommand CloseDetailsCommand { get; }

        ObservableCollection<IDetailsItemViewModel> DetailsItemViewModels { get; }

        ObservableCollection<DetailsItemSubMenuList> DetailsSubMenus { get; }

        IDetailsViewModelBase ItemDetailsViewModel { get; }
    }
}
