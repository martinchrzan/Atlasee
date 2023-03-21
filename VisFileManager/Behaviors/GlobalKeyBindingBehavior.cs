using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Settings;

namespace VisFileManager.Behaviors
{
    public enum WindowType { MainWindow, Gallery }

    public class GlobalKeyBindingBehavior : Behavior<Window>
    {
        private EyetrackerManager _eyetrackerManager;
        private ActivationInfoProvider _activationInfoProvider;
        private IHistoryManager _historyProvider;
        private Lazy<IUserSettings> _userSettings;
        private IMessenger _messenger;

        private bool _settingTrigger;
        private EyetrackingTriggerKey _eyetrackingKey;
        private Guid _changeEyeTrackingTriggerSubscription;

        public WindowType WindowType
        {
            get { return (WindowType)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }

        public static readonly DependencyProperty WindowTypeProperty = DependencyProperty.Register(
            "WindowType", typeof(WindowType), typeof(GlobalKeyBindingBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            _messenger.Unsubscribe(_changeEyeTrackingTriggerSubscription);
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _eyetrackerManager = Bootstraper.EyetrackerManager;
            _activationInfoProvider = Bootstraper.Container.GetExportedValue<ActivationInfoProvider>();
            _historyProvider = Bootstraper.Container.GetExportedValue<IHistoryManager>();
            _userSettings = Bootstraper.Container.GetExport<IUserSettings>();
            _messenger = Bootstraper.Container.GetExportedValue<IMessenger>();

            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            _changeEyeTrackingTriggerSubscription = _messenger.Subscribe<EyetrackingTriggersMessage>(MessageIds.ChangeEyetrackingKeyTrigger, (s) =>
            {
                _settingTrigger = true; _eyetrackingKey = s.TriggerKeyToChange;
            });
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.FocusedElement.GetType() == typeof(TextBox))
            {
                return;
            }

            if (_settingTrigger)
            {
                SetEyetrackingKeyTrigger(e);
                _settingTrigger = false;
                return;
            }

            if (e.Key == Key.Back)
            {
                _historyProvider.UndoCommand.Execute(null);
                return;
            }

            if (e.Key == Key.Escape)
            {
                if (WindowType == WindowType.Gallery)
                {
                    _messenger.Send(MessageIds.GalleryCloseWindow, true);
                }
                else if (WindowType == WindowType.MainWindow)
                {
                    _messenger.Send(MessageIds.CloseDetailsRequestMessage, true);
                }
            }

            if (e.Key == _activationInfoProvider.PrimaryActivationKey)
            {
                _activationInfoProvider.LastActivationKey = EyetrackingTriggerKey.Primary;
                SendActivation();
            }
            else if (e.Key == _activationInfoProvider.SecondaryActivationKey)
            {
                _activationInfoProvider.LastActivationKey = EyetrackingTriggerKey.Secondary;
                SendActivation();
            }
            else if (e.Key == _activationInfoProvider.ScrollActivationKey)
            {
                _activationInfoProvider.LastActivationKey = EyetrackingTriggerKey.Scroll;
                if (WindowType == WindowType.Gallery)
                {
                    _messenger.Send<bool>(MessageIds.GalleryZoomImage, true);
                }
                else if (WindowType == WindowType.MainWindow)
                {
                    _messenger.Send<bool>(MessageIds.ScrollRequestMessage, true);
                }
                SendActivation();
            }
            else if (e.Key == Key.Delete)
            {
                _activationInfoProvider.LastActivationKey = EyetrackingTriggerKey.Delete;
                SendActivation();
            }

            e.Handled = false;
        }

        private void SetEyetrackingKeyTrigger(KeyEventArgs e)
        {
            var userSettings = _userSettings.Value;
            switch (_eyetrackingKey)
            {
                case EyetrackingTriggerKey.Primary:
                    userSettings.PrimaryActivationKey.Value = e.Key.ToString();
                    if (userSettings.SecondaryActivationKey.Value == userSettings.PrimaryActivationKey.Value)
                    {
                        userSettings.SecondaryActivationKey.Value = string.Empty;
                    }
                    if (userSettings.ScrollActivationKey.Value == userSettings.PrimaryActivationKey.Value)
                    {
                        userSettings.ScrollActivationKey.Value = string.Empty;
                    }
                    break;
                case EyetrackingTriggerKey.Secondary:
                    userSettings.SecondaryActivationKey.Value = e.Key.ToString();
                    if (userSettings.SecondaryActivationKey.Value == userSettings.PrimaryActivationKey.Value)
                    {
                        userSettings.PrimaryActivationKey.Value = string.Empty;
                    }
                    if (userSettings.ScrollActivationKey.Value == userSettings.SecondaryActivationKey.Value)
                    {
                        userSettings.ScrollActivationKey.Value = string.Empty;
                    }
                    break;
                case EyetrackingTriggerKey.Scroll:
                    userSettings.ScrollActivationKey.Value = e.Key.ToString();
                    if (userSettings.ScrollActivationKey.Value == userSettings.PrimaryActivationKey.Value)
                    {
                        userSettings.PrimaryActivationKey.Value = string.Empty;
                    }
                    if (userSettings.ScrollActivationKey.Value == userSettings.SecondaryActivationKey.Value)
                    {
                        userSettings.SecondaryActivationKey.Value = string.Empty;
                    }
                    break;
            }

            _messenger.Send<bool>(MessageIds.EyetrackingKeyTriggerChanged, true);
        }

        private void SendActivation()
        {
            if (_eyetrackerManager.EyetrackingAvailability == Tobii.Interaction.Client.EyeXAvailability.Running)
            {
                _eyetrackerManager.Host.Commands.Input.SendActivation();
                _eyetrackerManager.Host.Commands.Input.SendActivationModeOff();
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }
    }
}
