using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VisFileManager.Behaviors
{
    public class ListFileItemsAppearBehavior : Behavior<Border>
    {
        private const int FixedItemHeight = 50;

        private bool _loaded;
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                AnimateAppear();
                _loaded = true;
            }
        }

        private void AnimateAppear()
        {
            ItemsControl parent = ItemsControlParentProvider.GetMainItemsControl(AssociatedObject);
            var container = parent.ItemContainerGenerator.ContainerFromItem(AssociatedObject.DataContext);
            var index = parent.ItemContainerGenerator.IndexFromContainer(container);

            var numberOfVisibleItems = parent.ActualHeight / FixedItemHeight;
            //no need for animation for non visible items
            if (index < numberOfVisibleItems)
            {
                DoubleAnimation opacity = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(200)));
                opacity.BeginTime = new TimeSpan(0, 0, 0, 0, 70 * index);
                opacity.EasingFunction = new QuarticEase();
                AssociatedObject.BeginAnimation(Border.OpacityProperty, opacity);

                DoubleAnimation width = new DoubleAnimation(0, parent.ActualWidth - 20, new Duration(TimeSpan.FromMilliseconds(200)));
                width.BeginTime = new TimeSpan(0, 0, 0, 0, 70 * index);
                width.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
                AssociatedObject.BeginAnimation(Border.WidthProperty, width);
            }
            else
            {
                AssociatedObject.Opacity = 1;
                AssociatedObject.Width = parent.ActualWidth - 20;
            }
        }
    }
}
