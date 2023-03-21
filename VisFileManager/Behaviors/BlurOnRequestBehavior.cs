using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;

namespace VisFileManager.Behaviors
{
    public class BlurOnRequestBehavior : Behavior<FrameworkElement>
    {
        private int _numberOfRequests;
        private int _clickDoesntCancelBlurCounter;
        private object _messageLock = new object();
        private bool _isEnabled;
        private IMessenger _messenger;

        public Grid BlurableGrid
        {
            get { return (Grid)GetValue(BlurableGridProperty); }
            set { SetValue(BlurableGridProperty, value); }
        }

        public static readonly DependencyProperty BlurableGridProperty = DependencyProperty.Register(
            "BlurableGrid", typeof(Grid), typeof(BlurOnRequestBehavior));

        public Border ClickAreaBorder
        {
            get
            {
                return (Border)GetValue(ClickAreaBorderProperty);
            }
            set
            {
                SetValue(ClickAreaBorderProperty, value);
            }
        }

        public static DependencyProperty ClickAreaBorderProperty = DependencyProperty.Register("ClickAreaBorder", typeof(Border), typeof(BlurOnRequestBehavior));
        private IInputElement _previouslyFocusedElement;

        private int ChangeNumberOfRequests(bool increase)
        {
            if (increase)
            {
                _numberOfRequests++;
            }
            else
            {
                _numberOfRequests--;
                if (_numberOfRequests < 0)
                {
                    _numberOfRequests = 0;
                }
            }
            return _numberOfRequests;
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            _messenger.Subscribe<BlurBackgroundMessage>(MessageIds.BlurBackgroundMessage, (message) =>
            {
                ProcessMessage(message);
            });

            ClickAreaBorder.PreviewMouseLeftButtonDown += ClickAreaBorder_PreviewMouseLeftButtonDown;
        }

        private void ClickAreaBorder_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_clickDoesntCancelBlurCounter == 0)
            {
                _messenger.Send(MessageIds.CloseDetailsRequestMessage, true);
            }
        }

        private void ProcessMessage(BlurBackgroundMessage message)
        {
            lock (_messageLock)
            {
                var numberOfRequestsToEnable = ChangeNumberOfRequests(message.EnableBlur);
                if (!message.ClickOnWillRemoveBlur && message.EnableBlur)
                {
                    _clickDoesntCancelBlurCounter++;
                }
                else if (!message.ClickOnWillRemoveBlur && !message.EnableBlur)
                {
                    _clickDoesntCancelBlurCounter--;
                }

                Dispatcher.Invoke(() =>
                {
                    if (numberOfRequestsToEnable > 0 && !_isEnabled)
                    {
                        //  IInputElement focusedControl = FocusManager.GetFocusedElement(this);
                        _previouslyFocusedElement = Keyboard.FocusedElement;
                        var blurEffect = new BlurEffect();
                        blurEffect.Radius = 0;

                        ClickAreaBorder.Visibility = Visibility.Visible;
                        var blurAnimation = new DoubleAnimation(10, new Duration(TimeSpan.FromMilliseconds(1000)));
                        blurAnimation.Completed += (sen, eve) =>
                        {
                            //   ClickAreaBorder.Visibility = Visibility.Visible;
                        };

                        BlurableGrid.Effect = blurEffect;
                        BlurableGrid.Effect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);


                        //Storyboard sb = (Storyboard)AssociatedObject.FindResource("blurAnimation");

                        //sb.Completed += (sen, eve) =>
                        //{
                        //    ClickAreaBorder.Visibility = Visibility.Visible;
                        //};

                        //sb.Begin();
                        _isEnabled = true;
                    }
                    else if (numberOfRequestsToEnable == 0 && _isEnabled)
                    {
                        var blurAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(200)));
                        blurAnimation.Completed += (sen, eve) =>
                        {
                            ClickAreaBorder.Visibility = Visibility.Hidden;
                            BlurableGrid.Effect = null;

                            Keyboard.Focus(_previouslyFocusedElement);
                        };

                        BlurableGrid.Effect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);

                        //Storyboard sb = (Storyboard)AssociatedObject.FindResource("deblurAnimation");

                        //sb.Completed += (sen, eve) =>
                        //{
                        //    ClickAreaBorder.Visibility = Visibility.Hidden;
                        //};

                        //sb.Begin();
                        _isEnabled = false;
                        // reset to default
                        _clickDoesntCancelBlurCounter = 0;
                    }
                });
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            ClickAreaBorder.PreviewMouseLeftButtonDown -= ClickAreaBorder_PreviewMouseLeftButtonDown;
            base.OnDetaching();
        }
    }
}
