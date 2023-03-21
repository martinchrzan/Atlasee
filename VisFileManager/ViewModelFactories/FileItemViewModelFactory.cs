using System;
using System.ComponentModel.Composition;
using System.IO;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;
using VisFileManager.ViewModels;

namespace VisFileManager.ViewModelFactories
{
    [Export(typeof(IFileItemViewModelFactory))]
    public class FileItemViewModelFactory : IFileItemViewModelFactory
    {
        private readonly IInvokeHelper _invokeHelper;
        private readonly IMessenger _messenger;
        private readonly FileInfoHelper _fileInfoHelper;
        private readonly IFileOperationsManager _fileOperationsManager;

        [ImportingConstructor]
        public FileItemViewModelFactory(IInvokeHelper invokeHelper, IMessenger messenger, FileInfoHelper fileInfoHelper, IFileOperationsManager fileOperationsManager)
        {
            _invokeHelper = invokeHelper;
            _messenger = messenger;
            _fileInfoHelper = fileInfoHelper;
            _fileOperationsManager = fileOperationsManager;
        }

        public IFileItemViewModel CreateFileItemViewModel(string fullPath, DateTime modifiedDate, bool searchResult)
        {
            return new FileItemViewModel(fullPath, modifiedDate, _invokeHelper, _messenger, _fileInfoHelper, searchResult, _fileOperationsManager);
        }

        public IDriveItemViewModel CreateFileItemViewModel(DriveInfo driveInfo)
        {
            return new DriveItemViewModel(driveInfo, _invokeHelper, _messenger);
        }
    }
}
