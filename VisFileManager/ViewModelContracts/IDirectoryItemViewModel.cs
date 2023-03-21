using System.Windows.Input;
using static VisFileManager.Validators.PathValidator;

namespace VisFileManager.ViewModelContracts
{
    public interface IDirectoryItemViewModel
    {
        string Name { get; }

        ICommand SelectedCommand { get; }

        ICommand PropertiesCommand { get; }

        PathType PathType { get; }
    }
}
