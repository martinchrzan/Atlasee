using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VisFileManager.Common;
using VisFileManager.Enums;
using VisFileManager.Extensions;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Settings;
using VisFileManager.ViewModelContracts;
using VisFileManager.ViewModelFactories;

namespace VisFileManager.ViewModels
{
    [Export(typeof(IMainFileViewModel))]
    public class MainFileViewModel : ViewModelBase, IMainFileViewModel
    {
        private readonly IFileItemViewModelFactory _fileItemViewModelFactory;
        private readonly IGlobalFileManager _globalFileManager;
        private readonly IMessenger _messenger;
        private readonly IDirectoryChangesNotifier _directoryChangesNotifier;
        private readonly ISearchViewModel _searchViewModel;
        private bool _isMyComputerView = true;
        private object _removeLock = new object();

        private SortingField _currentSortingField = SortingField.Name;
        private SortingDirection _currentSortingDirection = SortingDirection.Ascending;
        private bool _searchInProgress;
        private bool _loadingItems;

        [ImportingConstructor]
        public MainFileViewModel(IFileItemViewModelFactory fileItemViewModelFactory,
                                 IGlobalFileManager globalFileManager,
                                 IMessenger messenger,
                                 IUserSettings userSettings,
                                 IDirectoryChangesNotifier directoryChangesNotifier,
                                 IDiskDriveChangeMonitor diskDriveChangeMonitor,
                                 IDirectoriesToParentRemovalMonitor directoriesToParentRemovalMonitor,
                                 ISearchViewModel searchViewModel)
        {
            _fileItemViewModelFactory = fileItemViewModelFactory;
            _globalFileManager = globalFileManager;
            _messenger = messenger;
            _directoryChangesNotifier = directoryChangesNotifier;
            _searchViewModel = searchViewModel;

            FileItemViewModels = new RangeObservableCollection<IFileAndDriveItemViewModelBase>();

            _messenger.Subscribe<SortingRequest>(MessageIds.SortingRequest, NewSortingRequested);
            userSettings.ShowHiddenItems.PropertyChanged += async (s, e) => await LoadFileItems();
            _globalFileManager.CurrentPathChanged += GlobalFileManager_CurrentPathChanged;

            LoadFileItems();
            _directoryChangesNotifier.ItemCreated += DirectoryChangesNotifier_ItemCreated;
            _directoryChangesNotifier.ItemRemoved += DirectoryChangesNotifier_ItemRemoved;
            _directoryChangesNotifier.ItemRenamed += DirectoryChangesNotifier_ItemRenamed;


            diskDriveChangeMonitor.DiskDriveChanged += DiskDriveChangeMonitor_DiskDriveChanged;
            diskDriveChangeMonitor.Start();

            directoriesToParentRemovalMonitor.Start();

            _searchViewModel.SearchRequested += SearchViewModel_SearchRequested;
            _messenger.Subscribe<bool>(MessageIds.ClearSearchResults, ClearSearchResults);
        }

        public RangeObservableCollection<IFileAndDriveItemViewModelBase> FileItemViewModels { get; }

        public bool IsMyComputerView
        {
            get
            {
                return _isMyComputerView;
            }
            private set
            {
                _isMyComputerView = value;
                OnPropertyChanged();
            }
        }

        public bool SearchInProgress
        {
            get
            {
                return _searchInProgress;
            }
            private set
            {
                _searchInProgress = value;
                OnPropertyChanged();
            }
        }

        public bool LoadingItems
        {
            get
            {
                return _loadingItems;
            }
            set
            {
                _loadingItems = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler<int> NumberOfItemsChanged;

        private async void GlobalFileManager_CurrentPathChanged(object sender, bool e)
        {
            LoadingItems = true;
            await LoadFileItems();
        }

        private CancellationTokenSource _loadingThumbnailsCancellationTokenSource = new CancellationTokenSource();

        private async Task LoadFileItems(string filter = null, bool recursive = false, bool isSearch = false)
        {
            if (_loadingThumbnailsCancellationTokenSource != null)
            {
                _loadingThumbnailsCancellationTokenSource.Cancel();
            }

            await Task.Run(async () =>
            {
                await App.Current.Dispatcher.BeginInvoke((Action)(() => FileItemViewModels.Clear()), DispatcherPriority.Render);

                _messenger.Send(MessageIds.NewItemsLoaded, true);

                if (_globalFileManager.CurrentPath.PathType == Validators.PathValidator.PathType.MyComputer)
                {
                    foreach (var item in _globalFileManager.GetAllDrives().Where(it => it.IsReady).Select(it => _fileItemViewModelFactory.CreateFileItemViewModel(it)))
                    {
                        FileItemViewModels.AddWithoutNotification(item);
                    }

                    IsMyComputerView = true;
                    _directoryChangesNotifier.StopMonitoring();
                }
                else
                {
                    IOrderedEnumerable<IFileItemViewModel> directories = null;
                    IOrderedEnumerable<IFileItemViewModel> files = null;
                    IEnumerable<IFileItemViewModel> merged;

                    if (isSearch)
                    {
                        SearchInProgress = true;
                    }
                    var allEntries = await _globalFileManager.GetAllFileEntries(_globalFileManager.CurrentPath, filter, recursive);

                    var directoriesUnOrdered = allEntries.directories.Select(it => _fileItemViewModelFactory.CreateFileItemViewModel(it.FullName, it.LastWriteTime, isSearch));
                    var filesUnOrdered = allEntries.files.Select(it => _fileItemViewModelFactory.CreateFileItemViewModel(it.FullName, it.LastWriteTime, isSearch));
                    merged = GetOrderedFileItems(directories, files, directoriesUnOrdered, filesUnOrdered);


                    if (merged != null)
                    {
                        foreach (var item in merged)
                        {
                            FileItemViewModels.AddWithoutNotification(item);
                        }
                    }

                    IsMyComputerView = false;

                    _directoryChangesNotifier.MonitorDirectory(_globalFileManager.CurrentPath.Path);
                    _loadingThumbnailsCancellationTokenSource = new CancellationTokenSource();
                    Task.Run(async () =>
                    {
                        foreach (var item in FileItemViewModels)
                        {
                            if (_loadingThumbnailsCancellationTokenSource.IsCancellationRequested)
                            {
                                break;
                            }
                            (item as FileItemViewModel).Initialize();
                        }
                    }, _loadingThumbnailsCancellationTokenSource.Token);
                }
            });

            LoadingItems = false;
            App.Current.Dispatcher.Invoke(
                () =>
                FileItemViewModels.ReleaseNotification()
                );

            if (isSearch)
            {
                SearchInProgress = false;
            }
            OnNumberOfItemsChanged(FileItemViewModels.Count);


            //Task.Run(() =>
            //{
            //    Task.Delay(200);
            //    foreach (var item in FileItemViewModels)
            //    {
            //        var a = (item as FileItemViewModel).IconBig;
            //    }
            //});
        }



        private IEnumerable<IFileItemViewModel> GetOrderedFileItems(IOrderedEnumerable<IFileItemViewModel> directories, IOrderedEnumerable<IFileItemViewModel> files, IEnumerable<IFileItemViewModel> directoriesUnOrdered, IEnumerable<IFileItemViewModel> filesUnOrdered)
        {
            IEnumerable<IFileItemViewModel> merged = null;
            if (_currentSortingDirection == SortingDirection.Ascending)
            {

                if (_currentSortingField == SortingField.Name)
                {
                    directories = directoriesUnOrdered.OrderByNaturalAscending(it => it.Name);
                    files = filesUnOrdered.OrderByNaturalAscending(it => it.Name);
                }

                else if (_currentSortingField == SortingField.Modified)
                {
                    directories = directoriesUnOrdered.OrderBy(it => it.Modified);
                    files = filesUnOrdered.OrderBy(it => it.Modified);
                }

                else if (_currentSortingField == SortingField.Size)
                {
                    directories = directoriesUnOrdered.OrderBy(it => it.TotalSizeInBytes);
                    files = filesUnOrdered.OrderBy(it => it.TotalSizeInBytes);
                }

                // if ascending, directories first!
                merged = directories.Concat(files);

            }
            else if (_currentSortingDirection == SortingDirection.Descending)
            {
                if (_currentSortingField == SortingField.Name)
                {
                    directories = directoriesUnOrdered.OrderByNaturalDescending(it => it.Name);
                    files = filesUnOrdered.OrderByNaturalDescending(it => it.Name);
                }

                else if (_currentSortingField == SortingField.Modified)
                {
                    directories = directoriesUnOrdered.OrderByDescending(it => it.Modified);
                    files = filesUnOrdered.OrderByDescending(it => it.Modified);
                }

                else if (_currentSortingField == SortingField.Size)
                {
                    directories = directoriesUnOrdered.OrderByDescending(it => it.TotalSizeInBytes);
                    files = filesUnOrdered.OrderByDescending(it => it.TotalSizeInBytes);
                }

                // if ascending, directories first!
                merged = files.Concat(directories);
            }

            return merged;
        }

        private async void ClearSearchResults(bool clear)
        {
            if (clear)
            {
                await LoadFileItems();
            }
        }

        private async void SearchViewModel_SearchRequested(object sender, EventArgs e)
        {
            var searchTerm = _searchViewModel.SearchTerm;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // make it "contains" rather than equals search condition
                searchTerm = "*" + searchTerm + "*";
            }
            await LoadFileItems(searchTerm, _searchViewModel.IsRecursive, true);
        }

        private void OnNumberOfItemsChanged(int currentNumberOfItems)
        {
            NumberOfItemsChanged?.Invoke(this, currentNumberOfItems);
        }

        private void NewSortingRequested(SortingRequest sortingRequest)
        {
            _currentSortingDirection = sortingRequest.SortingDirection;
            _currentSortingField = sortingRequest.SortingField;

            LoadFileItems();
        }

        private void DirectoryChangesNotifier_ItemRenamed(object sender, ItemChangedInfo e)
        {
            var itemToChange = FileItemViewModels.FirstOrDefault(it => it.CachedFullPath == e.OldPath);

            if (itemToChange != null)
            {
                var index = FileItemViewModels.IndexOf(itemToChange);

                FileItemViewModels.Remove(itemToChange);

                var newFileItem = _fileItemViewModelFactory.CreateFileItemViewModel(e.NewPath, DateTime.Now, false);
                newFileItem.Initialize();
                FileItemViewModels.Insert(index, newFileItem);
            }
        }

        private async void DirectoryChangesNotifier_ItemRemoved(object sender, string e)
        {
            List<IFileAndDriveItemViewModelBase> itemsToRemove = new List<IFileAndDriveItemViewModelBase>();
            await Task.Run(async () =>
            {
                // we can get path if directory was removed, but not when file was removed so we need to check which one is missing
                if (string.IsNullOrEmpty(e))
                {
                    var entries = await _globalFileManager.GetAllFileEntries(_globalFileManager.CurrentPath);
                    var merged = entries.directories.Select(it => it.FullName).Concat(entries.files.Select(it => it.FullName));

                    var currentItems = FileItemViewModels.Select(it => it.CachedFullPath);

                    var toRemove = currentItems.Except(merged);

                    foreach (var removeItem in toRemove)
                    {
                        itemsToRemove.Add(FileItemViewModels.First(it => it.CachedFullPath == removeItem));
                    }
                }
                else
                {
                    var toRemove = FileItemViewModels.FirstOrDefault(it => it.CachedFullPath == e);
                    if (toRemove != null)
                    {
                        itemsToRemove.Add(FileItemViewModels.First(it => it.CachedFullPath == e));
                    }
                }
            });

            foreach (var removeItems in itemsToRemove)
            {
                FileItemViewModels.Remove(removeItems);
            }

            OnNumberOfItemsChanged(FileItemViewModels.Count);
        }

        private void DirectoryChangesNotifier_ItemCreated(object sender, ItemChangedInfo e)
        {
            var newItem = _fileItemViewModelFactory.CreateFileItemViewModel(e.NewPath, DateTime.Now, false);
            newItem.Initialize();
            FileItemViewModels.Add(newItem);
            OnNumberOfItemsChanged(FileItemViewModels.Count);
        }

        private async void DiskDriveChangeMonitor_DiskDriveChanged(object sender, EventArgs e)
        {
            if (IsMyComputerView)
            {
                await LoadFileItems();
            }
        }
    }
}
