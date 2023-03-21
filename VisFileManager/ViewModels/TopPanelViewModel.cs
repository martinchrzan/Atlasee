using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.Enums;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    [Export(typeof(ITopPanelViewModel))]
    public class TopPanelViewModel : ViewModelBase, ITopPanelViewModel
    {
        private readonly IGlobalFileManager _globalFileManager;
        private readonly IMessenger _messenger;
        private readonly IClipboardManager _clipboardManager;
        private readonly IHistoryManager _historyManager;
        private readonly IImagePreviewViewModel _imagePreviewViewModel;
        private FormattedPath _parentDirectoryPath;
        private FormattedPath _backDirectoryPath;
        private FormattedPath _forwardDirectoryPath;
        private bool _isGridViewActive = true;
        private SortingDirection _sortingDirection;
        private SortingField _sortingField;
        private bool _createNewItemViewModelEnabled = true;

        [ImportingConstructor]
        public TopPanelViewModel(IGlobalFileManager globalFileManager, IMessenger messenger, ICreateNewItemViewModel createNewItemViewModel, IClipboardManager clipboardManager, IHistoryManager historyManager,
            ISearchViewModel searchViewModel, IImagePreviewViewModel imagePreviewViewModel, IDialogHelper dialogHelper)
        {
            _globalFileManager = globalFileManager;
            _messenger = messenger;
            _clipboardManager = clipboardManager;
            _historyManager = historyManager;

            ParentDirectoryCommand = new RelayCommand((o) => ParentDirectoryPath != null, (o) => { MoveToParentDirectory(); });
            BackDirectoryCommand = _historyManager.UndoCommand;
            BackDirectoryPath = _historyManager.BackDirectoryPath;
            ForwardDirectoryCommand = _historyManager.RedoCommand;
            ForwardDirectoryPath = _historyManager.ForwardDirectoryPath;
            OpenGalleryCommand = new RelayCommand(() => { imagePreviewViewModel.Initialize(); dialogHelper.ShowGalleryWindow(imagePreviewViewModel as ViewModelBase); });

            SearchViewModel = searchViewModel;
            _imagePreviewViewModel = imagePreviewViewModel;
            UseGridView = new RelayCommand((o) => !_isGridViewActive, (o) => { messenger.Send(MessageIds.GridViewActive, true); _isGridViewActive = true; });
            UseListView = new RelayCommand((o) => _isGridViewActive, (o) => { messenger.Send(MessageIds.GridViewActive, false); _isGridViewActive = false; });

            PasteCommand = new RelayCommand((o) => clipboardManager.CanPaste, (o) => clipboardManager.Paste());
            PasteItems = clipboardManager.PasteItems;

            CreateNewItemViewModel = createNewItemViewModel;

            globalFileManager.CurrentPathChanged += GlobalFileManager_CurrentPathChanged;
            Revalidate();
        }

        private void Revalidate()
        {
            ForwardDirectoryPath = _historyManager.ForwardDirectoryPath;
            BackDirectoryPath = _historyManager.BackDirectoryPath;

            var parent = _globalFileManager.GetAllDirectoriesToParent(_globalFileManager.CurrentPath);
            if (parent.Any())
            {
                ParentDirectoryPath = parent.Reverse().Skip(1).Take(1).FirstOrDefault();
            }
            else
            {
                ParentDirectoryPath = null;
            }
        }

        private void GlobalFileManager_CurrentPathChanged(object sender, bool triggeredByHistory)
        {
            Revalidate();

            CreateNewItemViewModelEnabled = _globalFileManager.CurrentPath.PathType != PathValidator.PathType.MyComputer;
        }

        public ICommand ParentDirectoryCommand { get; }

        public ICommand BackDirectoryCommand { get; }

        public ICommand ForwardDirectoryCommand { get; }

        public ICommand UseGridView { get; }

        public ICommand UseListView { get; }
        
        public ICommand PasteCommand { get; }

        public ICommand OpenGalleryCommand { get; }

        public FormattedPath ParentDirectoryPath
        {
            get
            {
                return _parentDirectoryPath;
            }
            set
            {
                _parentDirectoryPath = value;
                OnPropertyChanged();
            }
        }

        public FormattedPath BackDirectoryPath
        {
            get
            {
                return _backDirectoryPath;
            }
            set
            {
                _backDirectoryPath = value;
                OnPropertyChanged();
            }
        }

        public FormattedPath ForwardDirectoryPath
        {
            get
            {
                return _forwardDirectoryPath;
            }
            set
            {
                _forwardDirectoryPath = value;
                OnPropertyChanged();
            }
        }

        public SortingDirection SortingDirection
        {
            get
            {
                return _sortingDirection;
            }
            set
            {
                _sortingDirection = value;
                OnPropertyChanged();
                RequestNewSorting();
            }
        }

        public SortingField SortingField
        {
            get
            {
                return _sortingField;
            }
            set
            {
                _sortingField = value;
                OnPropertyChanged();
                RequestNewSorting();
            }
        }

        public ICreateNewItemViewModel CreateNewItemViewModel { get; }

        public bool CreateNewItemViewModelEnabled
        {
            get
            {
                return _createNewItemViewModelEnabled;
            }
            private set
            {
                _createNewItemViewModelEnabled = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> PasteItems { get; } = new ObservableCollection<string>();

        public ISearchViewModel SearchViewModel { get; }

        private void MoveToParentDirectory()
        {
            _globalFileManager.SetCurrentPath(ParentDirectoryPath);
        }

        private void RequestNewSorting()
        {
            _messenger.Send(MessageIds.SortingRequest, SortingRequest.Create(SortingDirection, SortingField));
        }
    }
}
