using System;
using System.ComponentModel.Composition;
using System.Management;
using System.Threading;

namespace VisFileManager.FileSystemHelpers
{
    [Export(typeof(IDiskDriveChangeMonitor))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DiskDriveChangeMonitor : IDiskDriveChangeMonitor, IDisposable
    {
        private Thread _monitorThread;
        public event EventHandler DiskDriveChanged;
        private bool _started = false;
        private object _startLock = new object();

        public void Start()
        {
            lock (_startLock)
            {
                if (!_started)
                {
                    _monitorThread = new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_DiskDrive'");

                        ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
                        insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceChanged);
                        insertWatcher.Start();

                        WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_DiskDrive'");
                        ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
                        removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceChanged);
                        removeWatcher.Start();
                    });
                    _monitorThread.Start();
                    _started = true;
                }
            }
        }

        private void DeviceChanged(object sender, EventArrivedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                DiskDriveChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _monitorThread.Abort();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
