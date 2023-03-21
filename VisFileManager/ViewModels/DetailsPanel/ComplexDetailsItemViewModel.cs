using System.Collections.Generic;
using System.Collections.ObjectModel;
using VisFileManager.Common;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class ComplexDetailsItemViewModel : ViewModelBase, IComplexDetailsItemViewModel
    {
        public ComplexDetailsItemViewModel(string name, List<ISimpleDetailsItemViewModel> childList, int itemLevel)
        {
            ChildItems = new ObservableCollection<ISimpleDetailsItemViewModel>(childList);
            Name = name;
            Level = itemLevel;
        }

        public ObservableCollection<ISimpleDetailsItemViewModel> ChildItems { get; }

        public string Name { get; }

        public int Level { get; }
    }
}
