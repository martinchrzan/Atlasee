using Microsoft.Xaml.Behaviors;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VisFileManager.Behaviors
{
    public sealed class ButtonWithBorderPopupBehavior : Behavior<FrameworkElement>, IDisposable
    {
        private bool _opened;
        Timer _moveOverBorderDelayTimer;

        public Border PopupBorder
        {
            get
            {
                return (Border)GetValue(PopupBorderProperty);
            }
            set
            {
                SetValue(PopupBorderProperty, value);
            }
        }
        public static DependencyProperty PopupBorderProperty = DependencyProperty.Register("PopupBorder", typeof(Border), typeof(ButtonWithBorderPopupBehavior));

        public int DesiredHeight
        {
            get { return (int)GetValue(DesiredHeightProperty); }
            set { SetValue(DesiredHeightProperty, value); }
        }

        public static readonly DependencyProperty DesiredHeightProperty = DependencyProperty.Register(
            "DesiredHeight", typeof(int), typeof(ButtonWithBorderPopupBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            PopupBorder.MouseLeave += PopupBorder_MouseLeave;
        }

        private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_opened && !AssociatedObject.IsMouseOver)
            {
                _moveOverBorderDelayTimer = new Timer(CheckMouseOver, null, 500, Timeout.Infinite);
            }
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_opened && AssociatedObject.IsMouseOver)
            {
                DoubleAnimation openAnimation = new DoubleAnimation(DesiredHeight, new Duration(TimeSpan.FromMilliseconds(100)));

                openAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };

                PopupBorder.BeginAnimation(Border.HeightProperty, openAnimation);
                _opened = true;
            }
        }

        private void PopupBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseBorder();
        }

        private void CheckMouseOver(object state)
        {
            Dispatcher.Invoke(() =>
            CloseBorder());

        }

        private void CloseBorder()
        {
            if (_opened && !PopupBorder.IsMouseOver && !AssociatedObject.IsMouseOver)
            {
                DoubleAnimation closeAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(100)));

                closeAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };

                PopupBorder.BeginAnimation(Border.HeightProperty, closeAnimation);
                _opened = false;
            }

        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            base.OnDetaching();
            _moveOverBorderDelayTimer.Dispose();
        }

        public void Dispose()
        {
            _moveOverBorderDelayTimer.Dispose();
        }
    }
}
