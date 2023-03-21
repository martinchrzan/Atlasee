using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    public class FileItemViewModel : ViewModelBase, IFileItemViewModel
    {
        private readonly string _fullPath;
        private readonly IInvokeHelper _invokeHelper;
        private readonly IMessenger _messenger;
        private readonly FileInfoHelper _fileInfoHelper;
        private readonly IFileOperationsManager _fileOperationsManager;
        private string _name;
        private ImageSource _icon;
        private ImageSource _iconBig;
        private bool? _isDirectory;
        private string _sizeInKB = null;
        private string _type = null;

        public FileItemViewModel(string fullPath, DateTime modified, IInvokeHelper invokeHelper, IMessenger messenger, FileInfoHelper fileInfoHelper, bool searchResult, IFileOperationsManager fileOperationsManager)
        {
            _fullPath = fullPath;
            _invokeHelper = invokeHelper;
            _messenger = messenger;
            _fileInfoHelper = fileInfoHelper;
            IsSearchResult = searchResult;
            _fileOperationsManager = fileOperationsManager;
            _name = Path.GetFileName(_fullPath);
            CachedFullPath = _fullPath;
            Modified = modified;
            InitFromConstructor();
        }

        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = Path.GetFileName(_fullPath);
                }
                return _name;
            }
        }

        public FormattedPath FullFormattedPath { get; private set; }

        public bool IsDirectory
        {
            get
            {
                return (bool)_isDirectory;
            }
        }

        public ImageSource Icon
        {
            get
            {
                return _icon;
            }
        }

        public ImageSource IconBig
        {
            get
            {
                if(_iconBig == null)
                {
                    _iconBig = LoadIcon(true);
                }
                return _iconBig;
            }
        }

        public ImageSource Placeholder
        {
            get
            {
                return ResourcesProvider.GetFolderIcon();
            }
        }

        public ICommand OpenCommand => new RelayCommand(OpenFileItem);

        public ICommand OpenPropertiesCommand => new RelayCommand(OpenProperties);

        public ICommand RemoveCommand => new RelayCommand(RemoveItem);

        public DateTime Modified { get; }

        public string Type { get
            {
                if(_type == null)
                {
                    _type = _fileInfoHelper.GetSpecificFileType(FullFormattedPath).ToString(CultureInfo.InvariantCulture);
                }
                return _type;
            }
        }

        public ulong? TotalSizeInBytes
        {
            get
            {
                if(!IsDirectory)
                {
                    return _fileInfoHelper.GetFileSize(FullFormattedPath);
                }
                return 0;
            }
        }

        public string SizeInKB
        {
            get
            {
                // getting size of directory is slow and CPU heavy, can be displayed in details
                if (!IsDirectory)
                {
                    if (_sizeInKB == null)
                    {
                        _sizeInKB = _fileInfoHelper.GetFileSizeInKBFormatted(FullFormattedPath);
                    }
                    return _sizeInKB;
                }
                return string.Empty;
            }
        }

        public string CachedFullPath { get; }

        public bool IsSearchResult { get; }

        private void InitFromConstructor()
        {
            FullFormattedPath = FormattedPath.CreateFormattedPath(_fullPath);

            FileAttributes attr = File.GetAttributes(_fullPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                _isDirectory = true;
            }
            else { _isDirectory = false; }
        }

        public void Initialize()
        {
            if(_iconBig == null)
            {
                _iconBig = LoadIcon(true);
            }
        }

        private void OpenFileItem() => _invokeHelper.Invoke(FullFormattedPath);

        private void OpenProperties() => _messenger.Send(MessageIds.OpenFileDetailsMessage, new OpenFileDetailsMessage(FullFormattedPath, IsSearchResult));

        private void RemoveItem()
        {
            _fileOperationsManager.RemoveItems(new[] { FullFormattedPath}, new System.Threading.CancellationTokenSource());
        }

        private ImageSource LoadIcon(bool bigSize)
        {
            if (IsDirectory)
            {
                return ResourcesProvider.GetFolderIcon();
            }
            else
            {
                return BitmapHelpers.GetItemIcon(_fullPath, true, bigSize);
            }
        }
    }
}
