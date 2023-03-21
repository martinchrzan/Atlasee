using System.Collections.ObjectModel;

namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IComplexDetailsItemViewModel : IDetailsItemViewModel
    {
        ObservableCollection<ISimpleDetailsItemViewModel> ChildItems { get; }
    }
}
