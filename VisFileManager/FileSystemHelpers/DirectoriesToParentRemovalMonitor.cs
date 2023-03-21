using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using VisFileManager.Validators;

namespace VisFileManager.FileSystemHelpers
{
    // class to monitor all parent directories of current directory to detect if parent was removed or renamed and we must jump back to the closest existing parent
    [Export(typeof(IDirectoriesToParentRemovalMonitor))]
    public class DirectoriesToParentRemovalMonitor : IDirectoriesToParentRemovalMonitor, IDisposable
    {
        private readonly IGlobalFileManager _globalFileManager;
        private Dictionary<string, ShellObjectWatcher> watchers = new Dictionary<string, ShellObjectWatcher>();

        private bool _disposed = false;

        [ImportingConstructor]
        public DirectoriesToParentRemovalMonitor(IGlobalFileManager globalFileManager)
        {
            _globalFileManager = globalFileManager;
        }

        public void Start()
        {
            SetupAllWatcher();
            _globalFileManager.CurrentPathChanged += CurrentPathChanged;
        }

        private void CurrentPathChanged(object sender, bool e)
        {
            StopAllWatchers();
            SetupAllWatcher();
        }

        private void SetupAllWatcher()
        {
            if (_globalFileManager.CurrentPath.PathType == PathValidator.PathType.Directory)
            {
                foreach (var item in _globalFileManager.GetAllDirectoriesToParent(_globalFileManager.CurrentPath))
                {
                    if (item.PathType == PathValidator.PathType.Directory)
                    {
                        var watcher = new ShellObjectWatcher(ShellObject.FromParsingName(item.Path), false);
                        watcher.DirectoryDeleted += DirectoryDeleted;
                        watcher.DirectoryRenamed += Watcher_DirectoryRenamed;
                        watcher.Start();
                        watchers.Add(item.Path, watcher);
                    }
                }
            }
        }

        private void Watcher_DirectoryRenamed(object sender, ShellObjectRenamedEventArgs e)
        {
            if(watchers.ContainsKey(e.Path))
            {
                var closestParent = GetExistingDirectory(e.Path);
                _globalFileManager.SetCurrentPath(FormattedPath.CreateFormattedPath(closestParent));
            }
        }

        private void StopAllWatchers()
        {
            foreach(var watcher in watchers)
            {
                watcher.Value.Stop();
                watcher.Value.DirectoryDeleted -= DirectoryDeleted;
                watcher.Value.DirectoryRenamed -= Watcher_DirectoryRenamed;
                watcher.Value.Dispose();
            }

            watchers.Clear();
        }

        private void DirectoryDeleted(object sender, ShellObjectChangedEventArgs e)
        {
            if(watchers.ContainsKey(e.Path))
            {
                var closestParent = GetExistingDirectory(e.Path);
                _globalFileManager.SetCurrentPath(FormattedPath.CreateFormattedPath(closestParent));
            }
        }

        public static string GetExistingDirectory(string path)
        {
            var parent = Directory.GetParent(path);
            while (!parent.Exists)
            {
                if(parent == parent.Parent)
                {
                    break;
                }
                parent = parent.Parent;
            }
            return parent.FullName;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopAllWatchers();
                    _globalFileManager.CurrentPathChanged -= CurrentPathChanged;
                }
                // Release unmanaged resources.
                _disposed = true;
            }
        }

        ~DirectoriesToParentRemovalMonitor() { Dispose(false); }
    }
}
