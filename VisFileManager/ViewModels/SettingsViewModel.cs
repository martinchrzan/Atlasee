using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Settings;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    [Export(typeof(ISettingsViewModel))]
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly IUserSettings _userSettings;
        private readonly IMessenger _messenger;
        private readonly IBackgroundManager _backgroundManager;
        private bool _eyetrackingSettingsVisible;
        private Background _selectedBackground;

        [ImportingConstructor]
        public SettingsViewModel(IUserSettings userSettings, IMessenger messenger, IBackgroundManager backgroundManager)
        {
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            EyetrackingSettingsVisible = Bootstraper.EyetrackerManager.EyetrackingAvailability == Tobii.Interaction.Client.EyeXAvailability.Running;
            Bootstraper.EyetrackerManager.EyetrackingAvailabilityChanged += (s, e) => EyetrackingSettingsVisible = Bootstraper.EyetrackerManager.EyetrackingAvailability == Tobii.Interaction.Client.EyeXAvailability.Running;
            _userSettings = userSettings;
            _messenger = messenger;
            _backgroundManager = backgroundManager;
            _userSettings.PrimaryActivationKey.PropertyChanged += (s, e) => OnPropertyChanged(nameof(PrimaryKey));
            _userSettings.SecondaryActivationKey.PropertyChanged += (s, e) => OnPropertyChanged(nameof(SecondaryKey));
            _userSettings.ScrollActivationKey.PropertyChanged += (s, e) => OnPropertyChanged(nameof(ScrollKey));
            _userSettings.EyetrackingEnabled.PropertyChanged += (s, e) => OnPropertyChanged(nameof(EyetrackingEnabled));

            _messenger.Subscribe<bool>(MessageIds.EyetrackingKeyTriggerChanged, (s)=> { if (s) { SetButtonOverlayVisible = false; OnPropertyChanged(nameof(SetButtonOverlayVisible)); } });
            ChangePrimaryKeyCommand = new RelayCommand(() => ChangeKey(EyetrackingTriggerKey.Primary));
            ChangeSecondaryKeyCommand = new RelayCommand(() => ChangeKey(EyetrackingTriggerKey.Secondary));
            ChangeScrollKeyCommand = new RelayCommand(() => ChangeKey(EyetrackingTriggerKey.Scroll));

            _selectedBackground = backgroundManager.SelectedBackground;
            AvailableBackgrounds = backgroundManager.Backgrounds;
        }

        public ICommand OpenSettingsCommand { get; }

        public bool ShowHiddenItems { get => _userSettings.ShowHiddenItems.Value; set => _userSettings.ShowHiddenItems.Value = value; }

        public bool ShowDeleteConfirmationDialog { get => _userSettings.ShowDeleteConfirmationDialog.Value; set => _userSettings.ShowDeleteConfirmationDialog.Value = value; }

        public bool EyetrackingSettingsVisible
        {
            get
            {
                return _eyetrackingSettingsVisible;
            }
            set
            {
                _eyetrackingSettingsVisible = value;
                OnPropertyChanged();
            }
        }

        public string PrimaryKey
        {
            get { return _userSettings.PrimaryActivationKey.Value; }
        }

        public string SecondaryKey
        {
            get { return _userSettings.SecondaryActivationKey.Value; }
        }

        public string ScrollKey
        {
            get { return _userSettings.ScrollActivationKey.Value; }
        }

        public bool EyetrackingEnabled
        {
            get => _userSettings.EyetrackingEnabled.Value; set => _userSettings.EyetrackingEnabled.Value = value;
        }

        public uint SlideshowDurationInSec
        {
            get { return (uint)_userSettings.SlideshowDuration.Value; }
            set { _userSettings.SlideshowDuration.Value = (int)value; }
        }

        public ICommand ChangePrimaryKeyCommand { get; }

        public ICommand ChangeSecondaryKeyCommand { get; }

        public ICommand ChangeScrollKeyCommand { get; }

        public bool SetButtonOverlayVisible { get; private set; }

        public string ApplicationVersion { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        public Background SelectedBackground
        {
            get
            {
                return _selectedBackground;
            }
            set
            {
                _selectedBackground = value;
                _backgroundManager.SetBackground(value);
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Background> AvailableBackgrounds { get; }

        private void OpenSettings()
        {
            // load stuff
        }

        private void ChangeKey(Helpers.EyetrackingTriggerKey key)
        {
            SetButtonOverlayVisible = true;
            OnPropertyChanged(nameof(SetButtonOverlayVisible));

            _messenger.Send(MessageIds.ChangeEyetrackingKeyTrigger, new EyetrackingTriggersMessage(key));
        }
    }
}
