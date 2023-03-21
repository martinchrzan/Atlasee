namespace VisFileManager.Settings
{
    public interface IUserSettings
    {
        SettingItem<bool> ShowHiddenItems { get; } 

        SettingItem<bool> ShowDeleteConfirmationDialog { get; }

        SettingItem<string> PrimaryActivationKey { get; }

        SettingItem<string> SecondaryActivationKey { get; }

        SettingItem<string> ScrollActivationKey { get; }

        SettingItem<bool> EyetrackingEnabled { get; }

        SettingItem<int> SlideshowDuration { get; }

        SettingItem<string> BackgroundId { get; }
    }
}
