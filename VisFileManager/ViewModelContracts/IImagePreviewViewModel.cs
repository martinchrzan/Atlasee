using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisFileManager.Common;

namespace VisFileManager.ViewModelContracts
{
    public interface IImagePreviewViewModel
    {
        void Initialize();

        ICommand CloseCommand { get; }

        ICommand RotateCommand { get; }

        ICommand SlideshowCommand { get; }

        RangeObservableCollection<IImagePreviewItemViewModel> Images { get; }

        //converter on view will load full res image from path
        IImagePreviewItemViewModel SelectedImage { get; set; }

        //converter on view model will load low res image from path
        IImagePreviewItemViewModel SelectedImageLowRes { get; set; }

        IImagePreviewItemViewModel SelectedImageInList { get; set; }
    }
}
