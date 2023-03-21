using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using VisFileManager.ViewModelContracts.DetailsPanel;
using static VisFileManager.Helpers.InvokeHelper;

namespace VisFileManager.ViewModelFactories.DetailsPanel
{
    public interface IDetailsItemViewModelFactory
    {
        ISimpleDetailsItemViewModel CreateSimpleDetailsItemViewModel(ICommand command, BitmapSource icon, string name, int level, bool closeDetailsOnClick = true);

        ISimpleDetailsItemViewModel CreateOpenAsItemViewModel(OpenWithInfo openAsItem, string executeFile, int level, bool closeDetailsOnClick = true);

        IComplexDetailsItemViewModel CreateComplexDetailsItem(string name, List<ISimpleDetailsItemViewModel> childList, int level);
    }
}
