using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using VisFileManager.Common;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.ViewModelContracts.DetailsPanel;
using VisFileManager.ViewModels.DetailsPanel;

namespace VisFileManager.ViewModelFactories.DetailsPanel
{
    [Export(typeof(IDetailsItemViewModelFactory))]
    public class DetailsItemViewModelFactory : IDetailsItemViewModelFactory
    {
        private readonly IMessenger _messenger;

        [ImportingConstructor]
        public DetailsItemViewModelFactory(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public ISimpleDetailsItemViewModel CreateSimpleDetailsItemViewModel(ICommand command, BitmapSource icon, string name, int level, bool closeDetailsOnClick = true)
        {
            return new SimpleDetailsItemViewModel(command, icon, name, level, _messenger, closeDetailsOnClick);
        }

        public ISimpleDetailsItemViewModel CreateOpenAsItemViewModel(InvokeHelper.OpenWithInfo openAsItem, string executePath, int level, bool closeDetailsOnClick = true)
        {
            return new SimpleDetailsItemViewModel(
                new RelayCommand(() => InvokeHelper.InvokeOpenAsInfo(openAsItem, executePath)),
                BitmapHelpers.OpenAsInfoIconToBitmapSource(openAsItem),
                openAsItem.Name,
                level,
                _messenger,
                closeDetailsOnClick);
        }

        public IComplexDetailsItemViewModel CreateComplexDetailsItem(string name, List<ISimpleDetailsItemViewModel> childList, int level)
        {
            return new ComplexDetailsItemViewModel(name, childList, level);
        }
    }
}
