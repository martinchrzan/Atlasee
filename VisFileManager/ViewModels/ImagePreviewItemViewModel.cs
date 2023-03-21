using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    public class ImagePreviewItemViewModel : IImagePreviewItemViewModel
    {
        public ImagePreviewItemViewModel(string imagePath, Action<IImagePreviewItemViewModel> selectCommandAction)
        {
            SelectCommand = new RelayCommand((obj) =>
            {
                selectCommandAction(this);
            });
            Path = imagePath;
        }

        public string Path { get; }

        public ICommand SelectCommand { get; }
    }
}
