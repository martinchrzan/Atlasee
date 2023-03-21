using Microsoft.Xaml.Behaviors;
using System.Windows;
using VisFileManager.Helpers;

namespace VisFileManager.Behaviors
{
    public class ClearFocusOnClickBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            base.OnAttached();
        }

        private static void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WpfUtilities.FocusMainWindow();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        }
    }
}
