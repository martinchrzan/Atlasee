using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using VisFileManager.Settings;

namespace VisFileManager.Helpers
{
    public enum EyetrackingTriggerKey
    {
        Primary, Secondary, Scroll, Delete
    }

    [Export]
    public class ActivationInfoProvider
    {
        private readonly IUserSettings _userSettings;
   
        [ImportingConstructor]
        public ActivationInfoProvider(IUserSettings userSettings)
        {
            _userSettings = userSettings;

            LoadKeys();
            _userSettings.PrimaryActivationKey.PropertyChanged += ActivationKeyChanged;
            _userSettings.SecondaryActivationKey.PropertyChanged += ActivationKeyChanged;
            _userSettings.ScrollActivationKey.PropertyChanged += ActivationKeyChanged;
        }

        public EyetrackingTriggerKey LastActivationKey { get; set; }

        private void ActivationKeyChanged(object sender, PropertyChangedEventArgs e)
        {
            LoadKeys();
        }

        private void LoadKeys()
        {
            var converter = new KeyConverter();

            if (converter.ConvertFromString(_userSettings.PrimaryActivationKey.Value) != null)
            {
                PrimaryActivationKey = (Key)converter.ConvertFromString(_userSettings.PrimaryActivationKey.Value);
            }
            if (Enum.TryParse(_userSettings.SecondaryActivationKey.Value, out Key secondaryKey))
            {
                SecondaryActivationKey = secondaryKey;
            }
            if (Enum.TryParse(_userSettings.ScrollActivationKey.Value, out Key scrollKey))
            {
                ScrollActivationKey = scrollKey;
            }
        }

        public Key PrimaryActivationKey { get; private set; }

        public Key SecondaryActivationKey { get; private set; }

        public Key ScrollActivationKey { get; private set; }
    }
}
