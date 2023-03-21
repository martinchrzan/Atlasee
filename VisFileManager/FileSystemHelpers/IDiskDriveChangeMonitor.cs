using System;

namespace VisFileManager.FileSystemHelpers
{
    public interface IDiskDriveChangeMonitor
    {
        void Start();

        event EventHandler DiskDriveChanged;
    }
}
