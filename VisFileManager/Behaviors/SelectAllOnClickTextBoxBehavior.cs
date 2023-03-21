using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Input;

namespace VisFileManager.Behaviors
{
    public class SelectAllOnClickTextBoxBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.GotKeyboardFocus += AssociatedObject_GotKeyboardFocus;
        }

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // only select when "text area" - "TextBoxView" selected, not border
            if (!AssociatedObject.IsKeyboardFocusWithin && e.OriginalSource.GetType().Name == "TextBoxView")
            {
                e.Handled = true;
                AssociatedObject.Focus();
            }

            if (AssociatedObject.IsKeyboardFocusWithin && e.ClickCount == 3)
            {
                AssociatedObject.SelectAll();
            }
        }

        private void AssociatedObject_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            AssociatedObject.SelectAll();
        }
    }
}
