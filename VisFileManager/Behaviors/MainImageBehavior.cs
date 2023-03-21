using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VisFileManager.Behaviors
{
    public class MainImageBehavior : Behavior<Image>
    {
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
            AssociatedObject.Margin = new Thickness(100, 100, 100, 100);

            DoubleAnimation opacity = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(500)));
            opacity.BeginTime = new TimeSpan(0, 0, 0, 0, 700);
            opacity.EasingFunction = new QuarticEase();
            AssociatedObject.BeginAnimation(Image.OpacityProperty, opacity);

            ThicknessAnimation moveUp = new ThicknessAnimation(new Thickness(30, 10, 30, 10), new Duration(TimeSpan.FromMilliseconds(500)));
            moveUp.BeginTime = new TimeSpan(0, 0, 0, 0, 700);
            moveUp.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut };
            AssociatedObject.BeginAnimation(Image.MarginProperty, moveUp);
        }
    }
}
