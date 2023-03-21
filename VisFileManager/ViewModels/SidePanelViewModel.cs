using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;
using VisFileManager.ViewModelFactories;

namespace VisFileManager.ViewModels
{
    [Export(typeof(ISidePanelViewModel))]
    public class SidePanelViewModel : ViewModelBase, ISidePanelViewModel
    {
        private readonly IGlobalFileManager _globalFileManager;
        private readonly IDirectoryItemViewModelFactory _directoryItemViewModelFactory;
        private readonly IMainFileViewModel _mainFileViewModel;
        private FormattedPath _currentPath;
        private int _itemsInCurrentPath;

        [ImportingConstructor]
        public SidePanelViewModel(IGlobalFileManager globalFileManager, IDirectoryItemViewModelFactory directoryItemViewModelFactory, IMainFileViewModel mainFileViewModel, IDiskDriveChangeMonitor diskDriveChangeMonitor)
        {
            _globalFileManager = globalFileManager;
            _directoryItemViewModelFactory = directoryItemViewModelFactory;
            _mainFileViewModel = mainFileViewModel;

            Initialize();

            diskDriveChangeMonitor.DiskDriveChanged += (s, e) => CreateDrivesItems();
            diskDriveChangeMonitor.Start();
        }

        public ObservableCollection<IDirectoryItemViewModel> DirectoryItemViewModels { get; } = new ObservableCollection<IDirectoryItemViewModel>();

        public ObservableCollection<IDirectoryItemViewModel> Drives { get; } = new ObservableCollection<IDirectoryItemViewModel>();

        public string CurrentPath
        {
            get
            {
                return _currentPath.Path;
            }
            set
            {

                _currentPath = FormattedPath.CreateFormattedPath(value);
                _globalFileManager.SetCurrentPath(_currentPath);
                OnPropertyChanged();
            }
        }

        public int ItemsInCurrentPath
        {
            get
            {
                return _itemsInCurrentPath;
            }
            set
            {
                _itemsInCurrentPath = value;
                OnPropertyChanged();
            }
        }

        private void Initialize()
        {
            _globalFileManager.CurrentPathChanged += GlobalFileManager_CurrentPathChanged;

            CreateDirectoryItems();
            CreateDrivesItems();

            _currentPath = _globalFileManager.CurrentPath;

            ItemsInCurrentPath = _mainFileViewModel.FileItemViewModels.Count;
            _mainFileViewModel.NumberOfItemsChanged += (s, e) => ItemsInCurrentPath = e;
        }


        private void GlobalFileManager_CurrentPathChanged(object sender, bool e)
        {
            _currentPath = _globalFileManager.CurrentPath;
            OnPropertyChanged("CurrentPath");
            CreateDirectoryItems();
        }

        private void CreateDirectoryItems()
        {
            DirectoryItemViewModels.Clear();

            foreach (var directory in _globalFileManager.GetAllDirectoriesToParent(_globalFileManager.CurrentPath))
            {
                var directoryItem = _directoryItemViewModelFactory.CreateDirectoryItemViewModel(_globalFileManager, directory);
                DirectoryItemViewModels.Add(directoryItem);
            }
        }

        private void CreateDrivesItems()
        {
            Drives.Clear();
            foreach (var drive in _globalFileManager.GetAllDrives().Where(it => it.IsReady))
            {
                var directoryItem = _directoryItemViewModelFactory.CreateDriveItemViewModel(_globalFileManager, drive);
                Drives.Add(directoryItem);
            }
        }
    }
}
