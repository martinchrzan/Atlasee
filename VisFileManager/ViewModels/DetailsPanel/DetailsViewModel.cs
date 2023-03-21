using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.Controls;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;
using VisFileManager.ViewModelFactories;
using VisFileManager.ViewModelFactories.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    [Export(typeof(IDetailsViewModel))]
    public class DetailsViewModel : ViewModelBase, IDetailsViewModel
    {
        private readonly IInvokeHelper _invokeHelper;
        private readonly IMessenger _messenger;
        private readonly IDetailsItemViewModelFactory _detailsItemViewModelFactory;
        private readonly IFileAndDirectoryDetailsViewModelFactory _fileAndDirectoryDetailsViewModelFactory;
        private readonly IGlobalFileManager _globalFileManager;
        private readonly IClipboardManager _clipboardManager;
        private readonly IFileOperationsManager _fileOperationsManager;
        private readonly FileInfoHelper _fileInfoHelper;
        private bool _isOpened = false;
        private IDetailsViewModelBase _itemDetailsViewModel;

        [ImportingConstructor]
        public DetailsViewModel(IInvokeHelper invokeHelper, 
                                IMessenger messenger, 
                                IDetailsItemViewModelFactory detailsItemViewModelFactory, 
                                IFileAndDirectoryDetailsViewModelFactory fileAndDirectoryDetailsViewModelFactory,
                                IGlobalFileManager globalFileManager,
                                IClipboardManager clipboardManager,
                                IFileOperationsManager fileOperationsManager,
                                FileInfoHelper fileInfoHelper)
        {
            _invokeHelper = invokeHelper;
            _messenger = messenger;
            _detailsItemViewModelFactory = detailsItemViewModelFactory;
            _fileAndDirectoryDetailsViewModelFactory = fileAndDirectoryDetailsViewModelFactory;
            _globalFileManager = globalFileManager;
            _clipboardManager = clipboardManager;
            _fileOperationsManager = fileOperationsManager;
            _fileInfoHelper = fileInfoHelper;
            _messenger.Subscribe<OpenFileDetailsMessage>(MessageIds.OpenFileDetailsMessage, OpenFileDetails);
            _messenger.Subscribe<DriveInfo>(MessageIds.OpenDriveDetailsMessage, OpenDriveDetails);
            _messenger.Subscribe<bool>(MessageIds.CloseDetailsRequestMessage, CloseDetails);
        }

        public bool IsOpened
        {
            get
            {
                return _isOpened;
            }
            set
            {
                _isOpened = value;
                OnPropertyChanged();
                _messenger.Send(MessageIds.BlurBackgroundMessage, new BlurBackgroundMessage(value, true));
            }
        }

        public ICommand CloseDetailsCommand => new RelayCommand(() => IsOpened = false);

        public ObservableCollection<IDetailsItemViewModel> DetailsItemViewModels { get; } = new ObservableCollection<IDetailsItemViewModel>();

        public ObservableCollection<DetailsItemSubMenuList> DetailsSubMenus { get; } = new ObservableCollection<DetailsItemSubMenuList>();

        public IDetailsViewModelBase ItemDetailsViewModel
        {
            get { return _itemDetailsViewModel; }
            set
            {
                _itemDetailsViewModel = value;
                OnPropertyChanged();
            }
        }

        private void ClearMenus()
        {
            DetailsSubMenus.Clear();
            DetailsItemViewModels.Clear();
        }

        private void LoadOpen(FormattedPath selectedPath)
        {
            DetailsItemViewModels.Add(_detailsItemViewModelFactory.CreateSimpleDetailsItemViewModel(new RelayCommand(() => _invokeHelper.Invoke(selectedPath)), null, "Open", 0));
        }

        private void LoadOpenAs(FormattedPath selectedPath)
        {
            // open with item
            if (selectedPath.PathType == PathValidator.PathType.File)
            {
                List<ISimpleDetailsItemViewModel> openAsItems = new List<ISimpleDetailsItemViewModel>();
                foreach (var item in InvokeHelper.GetOpenWithInfo(selectedPath.ExtensionWithDot))
                {
                    openAsItems.Add(_detailsItemViewModelFactory.CreateOpenAsItemViewModel(item, selectedPath.Path, 1));
                }

                if (openAsItems.Count > 0)
                {
                    DetailsItemViewModels.Add(_detailsItemViewModelFactory.CreateComplexDetailsItem("Open with", openAsItems, 0));
                }
            }
        }

        private void AddRemoveItem()
        {
            DetailsItemViewModels.Add(_detailsItemViewModelFactory.CreateSimpleDetailsItemViewModel(
                new RelayCommand(() =>
                                _fileOperationsManager.RemoveItems(new[] { ItemDetailsViewModel.FullPath },
                                new System.Threading.CancellationTokenSource())),
                null, "Delete", 0));
        }

        private void AddCopyItem()
        {
            DetailsItemViewModels.Add(_detailsItemViewModelFactory.CreateSimpleDetailsItemViewModel(new RelayCommand(() => _clipboardManager.Copy(ItemDetailsViewModel.FullPath)), null, "Copy", 0));
        }

        private void AddCutItem()
        {
            DetailsItemViewModels.Add(_detailsItemViewModelFactory.CreateSimpleDetailsItemViewModel(new RelayCommand(() => _clipboardManager.Cut(ItemDetailsViewModel.FullPath)), null, "Cut", 0));
        }

        private void AddRenameItem()
        {
            DetailsItemViewModels.Add(_detailsItemViewModelFactory.CreateSimpleDetailsItemViewModel(new RelayCommand(() => _messenger.Send(MessageIds.EditNameMessage, true)), null, "Rename", 0, false));
        }

        private void AddOpenLocation(FormattedPath formattedPath)
        {
            var itemName = formattedPath.PathType == PathValidator.PathType.File ? "file" : "folder";
            DetailsItemViewModels.Add(_detailsItemViewModelFactory.CreateSimpleDetailsItemViewModel(new RelayCommand(() => _globalFileManager.SetCurrentPath(formattedPath.Parent)), null, "Open " + itemName+ " location", 0, false));
        }

        private void AddPropertiesItem(string fullPath)
        {
            DetailsItemViewModels.Add(_detailsItemViewModelFactory.CreateSimpleDetailsItemViewModel(new RelayCommand(() => _fileInfoHelper.ShowFileProperties(fullPath)), null, "Properties", 0));
        }

        private void OpenFileDetails(OpenFileDetailsMessage openFileDetailsMessage)
        {
            ItemDetailsViewModel = _fileAndDirectoryDetailsViewModelFactory.CreateFileOrDirectoryDetailsViewModel(openFileDetailsMessage.FormattedPath);

            IsOpened = true;
            ClearMenus();

            LoadOpen(openFileDetailsMessage.FormattedPath);
            LoadOpenAs(openFileDetailsMessage.FormattedPath);

            AddCopyItem();
            AddCutItem();

            AddRemoveItem();
            AddRenameItem();

            AddPropertiesItem(openFileDetailsMessage.FormattedPath.Path);

            if (openFileDetailsMessage.IsSearchResult)
            {
                AddOpenLocation(openFileDetailsMessage.FormattedPath);
            }
        }
        
        private void OpenDriveDetails(DriveInfo driveInfo)
        {
            ItemDetailsViewModel = _fileAndDirectoryDetailsViewModelFactory.CreateDriveDetailsViewModel(driveInfo);

            IsOpened = true;
            ClearMenus();
            LoadOpen(FormattedPath.CreateFormattedPath(driveInfo));
            AddPropertiesItem(driveInfo.RootDirectory.FullName);
        }

        private void CloseDetails(bool close)
        {
            if(close)
            {
                IsOpened = false;
            }
        }
    }
}
