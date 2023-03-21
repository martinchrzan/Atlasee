using System;

namespace VisFileManager.FileSystemHelpers
{
    public interface IDirectoryChangesNotifier
    {
        event EventHandler<ItemChangedInfo> ItemCreated;
        event EventHandler<ItemChangedInfo> ItemRenamed;
        event EventHandler<string> ItemRemoved;

        void MonitorDirectory(string path);

        void StopMonitoring();
    }
}
