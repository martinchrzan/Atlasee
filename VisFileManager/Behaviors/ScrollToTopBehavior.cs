using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using VisFileManager.Helpers;
using VisFileManager.Messenger;

namespace VisFileManager.Behaviors
{
    public class ScrollToTopBehavior : Behavior<FrameworkElement>
    {
        private ScrollViewer _scrollViewer;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            var messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            messenger.Subscribe<bool>(MessageIds.NewItemsLoaded, (a) =>
            {
                if (_scrollViewer == null)
                {
                    _scrollViewer = WpfUtilities.GetVisualChild<ScrollViewer>(AssociatedObject) as ScrollViewer;
                }
                _scrollViewer.ScrollToTop();
            });
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            base.OnDetaching();
        }
    }
}
