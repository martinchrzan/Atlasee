using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using VisFileManager.Helpers;
using VisFileManager.Messenger;

namespace VisFileManager.Behaviors
{
    public class EditOnRequestBehavior : Behavior<TextBox>
    {
        private IMessenger _messenger;
        private Guid _subscriptionId;
        private Guid _subscriptionId2;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            _messenger.Unsubscribe(_subscriptionId);
            _messenger.Unsubscribe(_subscriptionId2);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            _subscriptionId = _messenger.Subscribe<bool>(MessageIds.EditNameMessage, (message) =>
            {
                if (message)
                {
                    AssociatedObject.IsReadOnly = false;
                    AssociatedObject.Focus();
                }
            });

            _subscriptionId2 = _messenger.Subscribe<bool>(MessageIds.CloseDetailsRequestMessage, (message) =>
            {
                if (message)
                {
                    AssociatedObject.IsReadOnly = true;
                    WpfUtilities.FocusMainWindow();
                }
            });
        }
    }
}
