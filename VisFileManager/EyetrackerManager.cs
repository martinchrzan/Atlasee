using System;
using System.ComponentModel.Composition;
using Tobii.Interaction;
using Tobii.Interaction.Client;
using Tobii.Interaction.Wpf;
using VisFileManager.Settings;
using VisFileManager.Shared;

namespace VisFileManager
{
    [Export(typeof(EyetrackerManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class EyetrackerManager : IDisposable
    {
        private bool _isDisposed;
        private object _eyetrackingEnabledLock = new object();
        private EyetrackerMemoryLeakCleanerHack _eyetrackerMemoryLeakCleanerHack;
        private readonly IUserSettings _userSettings;

        public event EventHandler EyetrackingAvailabilityChanged;

        [ImportingConstructor]
        public EyetrackerManager(IUserSettings userSettings)
        {
            try
            {
                Host = new Host();

                Host.Context.ConnectionStateChanged += Context_ConnectionStateChanged;

                WpfInteractorAgent = Host.InitializeWpfAgent();
                _eyetrackerMemoryLeakCleanerHack = new EyetrackerMemoryLeakCleanerHack(this);
                _userSettings = userSettings;
                _userSettings.EyetrackingEnabled.PropertyChanged += EyetrackingEnabled_PropertyChanged;

                EnableConnection();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to initialize eyetracking", ex);
            }
        }

        public Host Host { get; private set; }

        public WpfInteractorAgent WpfInteractorAgent { get; private set; }

        public EyeXAvailability EyetrackingAvailability => Host.EyeXAvailability;

        public void Dispose()
        {
            _isDisposed = true;
            Host.Context.ConnectionStateChanged -= Context_ConnectionStateChanged;
            _userSettings.EyetrackingEnabled.PropertyChanged -= EyetrackingEnabled_PropertyChanged;
            Host.Dispose();
            WpfInteractorAgent.Dispose();
            _eyetrackerMemoryLeakCleanerHack.Dispose();
        }

        private void Context_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (!_isDisposed)
            {
                EyetrackingAvailabilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void EyetrackingEnabled_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lock (_eyetrackingEnabledLock)
            {
                EnableConnection();
            }
        }

        private void EnableConnection()
        {
            if (_userSettings.EyetrackingEnabled.Value)
            {
                Host.EnableConnection();
                Logger.LogInfo("Eyetracking connection enabled");
            }
            else
            {
                Host.DisableConnection();
                Logger.LogInfo("Eyetracking connection disabled");
            }
        }
    }
}
