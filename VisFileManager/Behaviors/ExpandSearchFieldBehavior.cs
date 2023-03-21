using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VisFileManager.Helpers;
using VisFileManager.Messenger;

namespace VisFileManager.Behaviors
{
    public class ExpandSearchFieldBehavior : Behavior<Canvas>
    {
        private const int SearchFieldWidthExpanded = 250;
        private bool _expanded = false;
        private IMessenger _messenger;

        public Path CloseSearchIcon
        {
            get { return (Path)GetValue(CloseSearchIconProperty); }
            set { SetValue(CloseSearchIconProperty, value); }
        }

        public static readonly DependencyProperty CloseSearchIconProperty = DependencyProperty.Register(
            "CloseSearchIcon", typeof(Path), typeof(ExpandSearchFieldBehavior));

        public Path SearchIcon
        {
            get { return (Path)GetValue(SearchIconProperty); }
            set { SetValue(SearchIconProperty, value); }
        }

        public FrameworkElement SearchField
        {
            get { return (FrameworkElement)GetValue(SearchFieldProperty); }
            set { SetValue(SearchFieldProperty, value); }
        }

        public TextBox SearchFieldTextBox
        {
            get { return (TextBox)GetValue(SearchFieldTextBoxProperty); }
            set { SetValue(SearchFieldTextBoxProperty, value); }
        }

        public static readonly DependencyProperty SearchFieldTextBoxProperty = DependencyProperty.Register(
            "SearchFieldTextBox", typeof(TextBox), typeof(ExpandSearchFieldBehavior));

        public static readonly DependencyProperty SearchFieldProperty = DependencyProperty.Register(
            "SearchField", typeof(FrameworkElement), typeof(ExpandSearchFieldBehavior));

        public static readonly DependencyProperty SearchIconProperty = DependencyProperty.Register(
            "SearchIcon", typeof(Path), typeof(ExpandSearchFieldBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            _messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var width = 0;
            if (_expanded)
            {
                SearchIcon.Visibility = Visibility.Visible;
                CloseSearchIcon.Visibility = Visibility.Collapsed;
                SearchFieldTextBox.Clear();
                _messenger.Send(MessageIds.ClearSearchResults, true);
                WpfUtilities.FocusMainWindow();
            }
            else
            {
                width = SearchFieldWidthExpanded;
                SearchIcon.Visibility = Visibility.Collapsed;
                CloseSearchIcon.Visibility = Visibility.Visible;
                SearchFieldTextBox.Focus();
            }

            DoubleAnimation widthAnimation = new DoubleAnimation(width, new Duration(TimeSpan.FromMilliseconds(300)));
            widthAnimation.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut };
            SearchField.BeginAnimation(Border.WidthProperty, widthAnimation);
            _expanded = !_expanded;
        }
    }
}
