using System;
using System.IO;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModelFactories
{
    public interface IFileItemViewModelFactory
    {
        IFileItemViewModel CreateFileItemViewModel(string fullPath, DateTime modifiedDate, bool searchResult);

        IDriveItemViewModel CreateFileItemViewModel(DriveInfo driveInfo);
    }
}
