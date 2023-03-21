using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface ISimpleDetailsItemViewModel : IDetailsItemViewModel
    {
        BitmapSource Icon { get; }

        ICommand InvokeCommand { get; }
    }
}
