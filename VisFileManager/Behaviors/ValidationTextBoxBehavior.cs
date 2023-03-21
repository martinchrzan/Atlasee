using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisFileManager.Helpers;
using VisFileManager.Validators;

namespace VisFileManager.Behaviors
{
    public class FileNameValidationTextBox : Behavior<TextBox>
    {
        public FormattedPath OriginalPath
        {
            get { return (FormattedPath)GetValue(OriginalPathProperty); }
            set { SetValue(OriginalPathProperty, value); }
        }

        public Border ErrorIndicatorBorder
        {
            get { return (Border)GetValue(ErrorIndicatorBorderProperty); }
            set { SetValue(ErrorIndicatorBorderProperty, value); }
        }

        public static DependencyProperty OriginalPathProperty = DependencyProperty.Register(
          "OriginalPath", typeof(FormattedPath), typeof(FileNameValidationTextBox));

        public static DependencyProperty ErrorIndicatorBorderProperty = DependencyProperty.Register(
            "ErrorIndicatorBorder", typeof(Border), typeof(FileNameValidationTextBox));

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
            AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
                WpfUtilities.FocusMainWindow();
                ((TextBox)sender).IsReadOnly = true;
            }
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {

            var isValid = FileNameValidator.ValidateName(AssociatedObject.Text, OriginalPath);
            ErrorIndicatorBorder.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
