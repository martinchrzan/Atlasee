using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;

namespace VisFileManager.Behaviors
{
    public class OpenBorderBehavior : Behavior<ButtonBase>
    {
        private IMessenger _messenger;
        private IThrottledActionInvoker _throttledActionInvoker;
        private bool _opened = false;

        public Border TargetBorder
        {
            get { return (Border)GetValue(TargetBorderProperty); }
            set { SetValue(TargetBorderProperty, value); }
        }
        public static readonly DependencyProperty TargetBorderProperty = DependencyProperty.Register(
          "TargetBorder", typeof(Border), typeof(OpenBorderBehavior));

        public int DesiredHeight
        {
            get { return (int)GetValue(DesiredHeightProperty); }
            set { SetValue(DesiredHeightProperty, value); }
        }

        public static readonly DependencyProperty DesiredHeightProperty = DependencyProperty.Register(
            "DesiredHeight", typeof(int), typeof(OpenBorderBehavior));

        public bool ExpandToFullScreen
        {
            get { return (bool)GetValue(ExpandToFullScreenProperty); }
            set { SetValue(ExpandToFullScreenProperty, value); }
        }

        public static readonly DependencyProperty ExpandToFullScreenProperty = DependencyProperty.Register(
            "ExpandToFullScreen", typeof(bool), typeof(OpenBorderBehavior));

        public bool BlurBackground
        {
            get { return (bool)GetValue(BlurBackgroundProperty); }
            set { SetValue(BlurBackgroundProperty, value); }
        }

        public static readonly DependencyProperty BlurBackgroundProperty = DependencyProperty.Register(
            "BlurBackground", typeof(bool), typeof(OpenBorderBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.Click += AssociatedObject_Click;

            _messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            _throttledActionInvoker = Bootstraper.Container.GetExportedValue<IThrottledActionInvokerFactory>().CreateThrottledActionInvoker();
            base.OnAttached();
        }

        private void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
            if (_opened)
            {
                ApplicationInfoProvider.GetApplicationMainArea().SizeChanged -= Application_SizeChanged;
                DoubleAnimation heightAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(300)));
                heightAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };

                TargetBorder.BeginAnimation(Border.HeightProperty, heightAnimation);
                _opened = false;

                if (BlurBackground)
                {
                    _messenger.Send(MessageIds.BlurBackgroundMessage, new BlurBackgroundMessage(false, false));
                }
            }
            else
            {
                var height = DesiredHeight;
                if (ExpandToFullScreen)
                {
                    height = (int)ApplicationInfoProvider.GetApplicationContentHeight();
                    ApplicationInfoProvider.GetApplicationMainArea().SizeChanged += Application_SizeChanged;
                }

                DoubleAnimation heightAnimation = new DoubleAnimation(height, new Duration(TimeSpan.FromMilliseconds(300)));
                heightAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };

                TargetBorder.BeginAnimation(Border.HeightProperty, heightAnimation);

                _opened = true;
                if (BlurBackground)
                {
                    _messenger.Send(MessageIds.BlurBackgroundMessage, new BlurBackgroundMessage(true, false));
                }

            }
        }

        private void Application_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _throttledActionInvoker.ScheduleAction(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    DoubleAnimation heightAnimation = new DoubleAnimation((int)ApplicationInfoProvider.GetApplicationContentHeight(), new Duration(TimeSpan.FromMilliseconds(1)));
                    TargetBorder.BeginAnimation(Border.HeightProperty, heightAnimation);
                });
            }, 10);
        }
    }
}
