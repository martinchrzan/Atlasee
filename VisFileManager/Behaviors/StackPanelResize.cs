using Microsoft.Xaml.Behaviors;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace VisFileManager.Behaviors
{
    public class StackPanelResize : Behavior<VirtualizingStackPanel>
    {
        DispatcherTimer _resizeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200), IsEnabled = false };

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;
            _resizeTimer.Stop();
            _resizeTimer.Tick -= _resizeTimer_Tick;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
            _resizeTimer.Tick += _resizeTimer_Tick;
        }

        private void _resizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.IsEnabled = false;

            var parent = ItemsControlParentProvider.GetMainItemsControl(AssociatedObject);
            ICollectionView view = CollectionViewSource.GetDefaultView(parent.ItemsSource);
            view.Refresh();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }
    }
}
