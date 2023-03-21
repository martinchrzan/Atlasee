using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VisFileManager.Behaviors
{
    public class DialogBehavior : Behavior<FrameworkElement>
    {
        public Button ButtonToInvokeOnEsc
        {
            get { return (Button)GetValue(ButtonToInvokeOnEscProperty); }
            set { SetValue(ButtonToInvokeOnEscProperty, value); }
        }

        public static DependencyProperty ButtonToInvokeOnEscProperty = DependencyProperty.Register(
            "ButtonToInvokeOnEsc", typeof(Button), typeof(DialogBehavior));

        public Button DefaultButton
        {
            get { return (Button)GetValue(DefaultButtonProperty); }
            set { SetValue(DefaultButtonProperty, value); }
        }

        public static DependencyProperty DefaultButtonProperty = DependencyProperty.Register(
            "DefaultButton", typeof(Button), typeof(DialogBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            DefaultButton.Focus();
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ButtonToInvokeOnEsc.Command.Execute(null);
            }
        }
    }
}
