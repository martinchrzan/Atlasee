using System.ComponentModel.Composition;

namespace VisFileManager.Settings
{
    [Export(typeof(IUserSettings))]
    public class UserSettings : IUserSettings
    {
        public UserSettings()
        {
            var settings = Properties.Settings.Default;
            ShowHiddenItems = new SettingItem<bool>(settings.ShowHiddenItems, (currentValue) => { settings.ShowHiddenItems = currentValue; SaveSettings(); });

            ShowDeleteConfirmationDialog = new SettingItem<bool>(settings.ShowDeleteConfirmationDialog, (currentValue) => { settings.ShowDeleteConfirmationDialog = currentValue; SaveSettings(); });

            PrimaryActivationKey = new SettingItem<string>(settings.PrimaryActivationKey, (currentValue) => { settings.PrimaryActivationKey = currentValue; SaveSettings(); });

            SecondaryActivationKey = new SettingItem<string>(settings.SecondaryActivationKey, (currentValue) => { settings.SecondaryActivationKey = currentValue; SaveSettings(); });

            ScrollActivationKey = new SettingItem<string>(settings.ScrollActivationKey, (currentValue) => { settings.ScrollActivationKey = currentValue; SaveSettings(); });

            EyetrackingEnabled = new SettingItem<bool>(settings.EyetrackingEnabled, (currentValue) => { settings.EyetrackingEnabled = currentValue; SaveSettings(); });

            SlideshowDuration = new SettingItem<int>(settings.SlideshowDurationInSec, (currentValue) => { settings.SlideshowDurationInSec = currentValue; SaveSettings(); });

            BackgroundId = new SettingItem<string>(settings.BackgroundId, (currentValue) => { settings.BackgroundId = currentValue; SaveSettings(); });
        }

        public SettingItem<bool> ShowHiddenItems { get; }

        public SettingItem<bool> ShowDeleteConfirmationDialog { get; }

        public SettingItem<string> PrimaryActivationKey { get; }

        public SettingItem<string> SecondaryActivationKey { get; }

        public SettingItem<string> ScrollActivationKey { get; }

        public SettingItem<bool> EyetrackingEnabled { get; }

        public SettingItem<int> SlideshowDuration { get; }

        public SettingItem<string> BackgroundId { get; }

        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }
    }
}
