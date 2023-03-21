using System;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModelFactories
{
    public interface IImagePreviewItemViewModelFactory
    {
        IImagePreviewItemViewModel CreateImagePreviewItemViewModel(string imagePath, Action<IImagePreviewItemViewModel> selectCommandAction);
    }
}
