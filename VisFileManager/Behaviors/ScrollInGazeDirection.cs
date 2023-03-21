using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Tobii.Interaction;
using VisFileManager.Helpers;
using VisFileManager.Messenger;

namespace VisFileManager.Behaviors
{
    public class ScrollInGazeDirection : Behavior<FrameworkElement>
    {
        private ScrollViewer _scrollViewer;
        private EyetrackerManager _eyetrackerManager;
        private FixationDataStream _stream;
        private double _lastY;


        public static DependencyProperty VerticalOffsetProperty =
         DependencyProperty.RegisterAttached("VerticalOffset",
                                             typeof(double),
                                             typeof(ScrollInGazeDirection),
                                             new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }
        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }
        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            Detach();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            _stream.Next -= _stream_Next;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _eyetrackerManager = Bootstraper.EyetrackerManager;

            _stream = _eyetrackerManager.Host.Streams.CreateFixationDataStream(Tobii.Interaction.Framework.FixationDataMode.Sensitive);
            _stream.Next += _stream_Next;

            var messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            messenger.Subscribe<bool>(MessageIds.ScrollRequestMessage, (a) =>
            {
                if (_scrollViewer == null)
                {
                    _scrollViewer = WpfUtilities.GetVisualChild<ScrollViewer>(AssociatedObject) as ScrollViewer;
                }

                var lookingAtBottom = LookingAtBottomPartOfApplication();
                if (lookingAtBottom == true)
                {
                    AnimateScroll(_scrollViewer, _scrollViewer.VerticalOffset + _scrollViewer.ActualHeight);
                }
                else if (lookingAtBottom == false)
                {
                    AnimateScroll(_scrollViewer, _scrollViewer.VerticalOffset - _scrollViewer.ActualHeight);
                }
            });
        }

        private void AnimateScroll(ScrollViewer scrollViewer, double ToValue)
        {
            DoubleAnimation verticalAnimation = new DoubleAnimation();
            verticalAnimation.From = scrollViewer.VerticalOffset;
            verticalAnimation.To = ToValue;
            verticalAnimation.DecelerationRatio = .3;
            verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(600));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(verticalAnimation);
            Storyboard.SetTarget(verticalAnimation, scrollViewer);
            Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollInGazeDirection.VerticalOffsetProperty));
            storyboard.Begin();
        }

        private void _stream_Next(object sender, StreamData<FixationData> e)
        {
            _lastY = e.Data.Y;
        }

        private bool? LookingAtBottomPartOfApplication()
        {
            var top = ApplicationInfoProvider.GetApplicationTop();
            var height = ApplicationInfoProvider.GetApplicationContentHeight();
            if (_lastY >= top + 0.7 * height)
            {
                return true;
            }
            else if (_lastY <= top + 0.4 * height)
            {
                return false;
            }
            return null;
        }
    }
}
