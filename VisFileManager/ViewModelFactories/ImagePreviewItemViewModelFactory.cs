using System;
using System.ComponentModel.Composition;
using VisFileManager.ViewModelContracts;
using VisFileManager.ViewModels;

namespace VisFileManager.ViewModelFactories
{
    [Export(typeof(IImagePreviewItemViewModelFactory))]
    public class ImagePreviewItemViewModelFactory : IImagePreviewItemViewModelFactory
    {
        public IImagePreviewItemViewModel CreateImagePreviewItemViewModel(string imagePath, Action<IImagePreviewItemViewModel> selectCommandAction)
        {
            return new ImagePreviewItemViewModel(imagePath, selectCommandAction);
        }
    }
}
