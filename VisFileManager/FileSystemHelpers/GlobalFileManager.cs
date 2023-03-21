using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VisFileManager.Helpers;
using VisFileManager.Settings;
using VisFileManager.Shared;
using VisFileManager.Validators;
using static VisFileManager.Validators.PathValidator;

namespace VisFileManager.FileSystemHelpers
{
    [Export(typeof(IGlobalFileManager))]
    public class GlobalFileManager : IGlobalFileManager
    {
        private readonly object _currentPathLock = new object();
        private readonly object _selectedItemsLock = new object();
        private readonly IUserSettings _userSettings;
        private FormattedPath _currentPath = FormattedPath.CreateFormattedPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        private List<FormattedPath> _selectedItems;

        [ImportingConstructor]
        public GlobalFileManager(IUserSettings userSettings)
        {
            _userSettings = userSettings;
        }

        public event EventHandler<bool> CurrentPathChanged;
        public event EventHandler SelectedItemsChanged;

        public void SetCurrentPath(FormattedPath currentPath, bool triggeredByHistory = false)
        {
            lock (_currentPathLock)
            {
                _currentPath = currentPath;
                OnCurrentPathChanged(triggeredByHistory);
            }
        }

        public void SetSelectedItems(List<FormattedPath> selectedItems)
        {
            lock (_selectedItemsLock)
            {
                _selectedItems = selectedItems;
                OnSelectedItemChanged();
            }
        }

        public FormattedPath CurrentPath
        {
            get
            {
                lock (_currentPathLock)
                {
                    return _currentPath;
                }
            }
        }

        public List<FormattedPath> SelectedItems
        {
            get
            {
                lock (_selectedItemsLock)
                {
                    return _selectedItems;
                }
            }
        }

        public IEnumerable<FormattedPath> GetAllDirectoriesToParent(FormattedPath formattedPath)
        {
            var nodes = new List<FormattedPath>() { formattedPath };
            if(formattedPath == null)
            {
                Logger.LogWarning("Formatted path is empty");
                return nodes;
            }

            if (formattedPath.PathType == PathType.MyComputer)
            {
                return nodes;
            }

            var parent = Directory.GetParent(formattedPath.Path);

            while (parent != null)
            {
                nodes.Add(FormattedPath.CreateFormattedPath(parent.FullName));
                parent = parent.Parent;
            }

            nodes.Add(FormattedPath.CreateFormattedPath(string.Empty));

            nodes.Reverse();
            return nodes;
        }


        public IEnumerable<DriveInfo> GetAllDrives()
        {
            var driveInfo = new List<DriveInfo>();
            foreach (var logicalDrivess in Environment.GetLogicalDrives())
            {
                driveInfo.Add(new DriveInfo(logicalDrivess));
            }

            return driveInfo;
        }

        public Task<(IEnumerable<DirectoryInfo> directories, IEnumerable<FileInfo> files)> GetAllFileEntries(FormattedPath rootPath, string filter = null, bool recursive = false)
        {
            return Task.Run(() =>
            {
                try
                {
                    var dir = new DirectoryInfo(rootPath.Path);
                    IEnumerable<DirectoryInfo> directories;
                    IEnumerable<FileInfo> files;

                    if (!string.IsNullOrEmpty(filter))
                    {
                        directories = SafeFilesEnumerator.EnumerateDirectories(dir, filter, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                        files = SafeFilesEnumerator.EnumerateFiles(dir, filter, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    }
                    else
                    {
                        directories = dir.EnumerateDirectories();
                        files = dir.EnumerateFiles();
                    }

                    if (!_userSettings.ShowHiddenItems.Value)
                    {
                        directories = directories.Where(it => !it.Attributes.HasFlag(FileAttributes.Hidden) && !it.Attributes.HasFlag(FileAttributes.System));
                        files = files.Where(it => !it.Attributes.HasFlag(FileAttributes.Hidden) && !it.Attributes.HasFlag(FileAttributes.System));
                    }

                    return (directories, files);
                }
                catch(Exception ex)
                {
                    Logger.LogError("Failed to get all file and directory entries in " + rootPath, ex);
                    return (new List<DirectoryInfo>(), new List<FileInfo>());
                }
            });
        }

        public int GetNumberOfItemsInPath(FormattedPath path)
        {
            var numberOfItems = 0;
            if(path == null)
            {
                Logger.LogWarning("Path is empty in Get number of items in path call");
                return numberOfItems;
            }
            if (path.PathType == PathType.Directory)
            {
                try
                {
                    var info = new DirectoryInfo(path.Path);
                    numberOfItems = info.EnumerateFiles().Count();
                    numberOfItems += info.EnumerateDirectories().Count();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to get number of items in path" + Environment.NewLine + ex.Message);
                }
            }
            return numberOfItems;
        }

        private void OnCurrentPathChanged(bool triggeredByHistory)
        {
            CurrentPathChanged?.Invoke(this, triggeredByHistory);
        }

        private void OnSelectedItemChanged()
        {
            SelectedItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
