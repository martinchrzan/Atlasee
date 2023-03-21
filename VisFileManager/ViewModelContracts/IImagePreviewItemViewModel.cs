using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisFileManager.ViewModelContracts
{
    public interface IImagePreviewItemViewModel
    {
        string Path { get; }

        ICommand SelectCommand { get; }
    }
}
