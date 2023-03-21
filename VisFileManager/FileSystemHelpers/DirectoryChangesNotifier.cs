using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisFileManager.FileSystemHelpers
{
    [Export(typeof(IDirectoryChangesNotifier))]
    public class DirectoryChangesNotifier : IDirectoryChangesNotifier, IDisposable
    {
        private ShellObjectWatcher _watcher;
        private bool disposed;

        public event EventHandler<ItemChangedInfo> ItemCreated;
        public event EventHandler<ItemChangedInfo> ItemRenamed;
        public event EventHandler<string> ItemRemoved;

        public void MonitorDirectory(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return;
            }

            StopMonitoring();

            _watcher = new ShellObjectWatcher(ShellObject.FromParsingName(path), false);

            SubscribeEvents(_watcher);

            _watcher.Start();
        }

        private void SubscribeEvents(ShellObjectWatcher watcher)
        {
            watcher.DirectoryCreated += Watcher_ItemCreated;
            watcher.DirectoryDeleted += Watcher_ItemDeleted;
            watcher.DirectoryRenamed += Watcher_ItemRenamed;

            watcher.ItemCreated += Watcher_ItemCreated;
            watcher.ItemDeleted += Watcher_ItemDeleted;
            watcher.ItemRenamed += Watcher_ItemRenamed;
        }

        private void UnsubscribeRemoveEvents(ShellObjectWatcher watcher)
        {
            watcher.DirectoryCreated -= Watcher_ItemCreated;
            watcher.DirectoryDeleted -= Watcher_ItemDeleted;
            watcher.DirectoryRenamed -= Watcher_ItemDeleted;

            watcher.ItemCreated -= Watcher_ItemCreated;
            watcher.ItemDeleted -= Watcher_ItemDeleted;
            watcher.ItemRenamed -= Watcher_ItemDeleted;
        }
        
        private void Watcher_ItemRenamed(object sender, ShellObjectRenamedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RaiseRenamed(e.NewPath, e.Path);
            });
        }

        private void Watcher_ItemDeleted(object sender, ShellObjectChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RaiseRemoved(e.Path);
            });
        }

        private void Watcher_ItemCreated(object sender, ShellObjectChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RaiseCreated(e.Path);
            });
        }
        
        private void RaiseCreated(string newPath)
        {
            ItemCreated?.Invoke(this, new ItemChangedInfo(newPath));
        }

        private void RaiseRenamed(string newPath, string oldPath)
        {
            ItemRenamed?.Invoke(this, new ItemChangedInfo(newPath, oldPath));
        }

        private void RaiseRemoved(string path)
        {
            ItemRemoved?.Invoke(this, path);
        }

        public void StopMonitoring()
        {
            if (_watcher != null)
            {
                _watcher.Stop();
                UnsubscribeRemoveEvents(_watcher);
                _watcher.Dispose();
                _watcher = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopMonitoring();
                }
                // Release unmanaged resources.
                disposed = true;
            }
        }

        ~DirectoryChangesNotifier() { Dispose(false); }
    }
}
