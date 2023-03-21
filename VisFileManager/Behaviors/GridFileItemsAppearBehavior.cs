using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VisFileManager.Behaviors
{
    public class GridFileItemsAppearBehavior : Behavior<Border>
    {
        private const int FixedItemWidthAndHeight = 125;
        private bool _loaded;
        private ItemsControl _parent;
        private DependencyObject _container;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            Detach();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                _parent = ItemsControlParentProvider.GetMainItemsControl(AssociatedObject);
                var numberOfVisibleHorizontalItems = _parent.ActualWidth / FixedItemWidthAndHeight;
                var numberOfVisibleVertialItems = _parent.ActualHeight / FixedItemWidthAndHeight;

                _container = _parent.ItemContainerGenerator.ContainerFromItem(AssociatedObject.DataContext);

                var index = _parent.ItemContainerGenerator.IndexFromContainer(_container);

                //no need for animation for non visible items
                if (index < numberOfVisibleHorizontalItems * numberOfVisibleVertialItems)
                {
                    AssociatedObject.Opacity = 0.0;
                    AssociatedObject.RenderTransform = new TranslateTransform(10, 50);

                    DoubleAnimation opacity = new DoubleAnimation(0.9, new Duration(TimeSpan.FromMilliseconds(400)));
                    opacity.BeginTime = new TimeSpan(0, 0, 0, 0, 20 * index + 200);
                    opacity.EasingFunction = new QuarticEase();
                    AssociatedObject.BeginAnimation(Border.OpacityProperty, opacity);

                    //var moveUp = new TranslateTransform();
                    DoubleAnimation moveUp = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(400)));
                    moveUp.BeginTime = new TimeSpan(0, 0, 0, 0, 20 * index + 200);
                    moveUp.EasingFunction = new QuarticEase();
                    AssociatedObject.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveUp);
                    AssociatedObject.RenderTransform.BeginAnimation(TranslateTransform.XProperty, moveUp);
                }
                else
                {
                    //AssociatedObject.Opacity =0.9;
                    //AssociatedObject.RenderTransform = new TranslateTransform(0, 0);
                }

                _loaded = true;
            }
        }
    }
}
