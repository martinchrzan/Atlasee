﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Tobii.Interaction;
using Tobii.Interaction.Wpf;
using VisFileManager.Messenger;

namespace VisFileManager.Controls
{
    public class ZoomBorder : Border
    {
        private const int ZoomAndTranslateAnimationDurationInMs = 200;
        private const int QuickAnimationDurationInMs = 10;
        private const double ZoomFactor = 1.3;

        private UIElement _child = null;
        private Point _origin;
        private Point _start;

        private bool _translateAvailable = true;
        private DoubleAnimation _animationX;
        private IMessenger _messenger;
        private Guid _prepareNewImageSubscriptionId;
        private Guid _rotateLeftSubscriptionId;
        private Guid _zoomSubscriptionId;
        private double _lastY;
        private double _lastX;
        private WpfInteractor _interactor;
        private GazePointDataStream _stream;
        private DoubleAnimation _disappearAnimation;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        private RotateTransform GetRotateTransform(UIElement element)
        {
            return (RotateTransform)((TransformGroup)element.RenderTransform)
                 .Children.FirstOrDefault(tr => tr is RotateTransform);
        }

        public FrameworkElement GazeVisualizationControl
        {
            get; set;
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != Child)
                    Initialize(value);
                var grid = new Grid();
                grid.Children.Add(value);
                grid.Children.Add(GazeVisualizationControl);

                base.Child = grid;
            }
        }

        public void Initialize(UIElement element)
        {
            _child = element;
            if (_child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                TranslateTransform tt = new TranslateTransform();
                RotateTransform rt = new RotateTransform();
               
                group.Children.Add(st);
                group.Children.Add(tt);
                group.Children.Add(rt);
                _child.RenderTransform = group;
                _child.RenderTransformOrigin = new Point(0.5, 0.5);
                MouseWheel += Child_MouseWheel;
                MouseLeftButtonDown += Child_MouseLeftButtonDown;
                MouseLeftButtonUp += Child_MouseLeftButtonUp;
                MouseMove += Child_MouseMove;
                PreviewMouseRightButtonDown += Child_PreviewMouseRightButtonDown;
            }
            Unloaded += ZoomBorder_Unloaded;

            _messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            _prepareNewImageSubscriptionId = _messenger.Subscribe<bool>(MessageIds.GalleryPrepareForNewImage, (o) => { Dispatcher.Invoke(() => { Reset(true); }); });
            _rotateLeftSubscriptionId = _messenger.Subscribe<bool>(MessageIds.GalleryRotateImage, (o) => { Rotate(); });
            _zoomSubscriptionId = _messenger.Subscribe<bool>(MessageIds.GalleryZoomImage, (zoomIn) => ZoomIn(zoomIn));

            var eyetrackerManager = Bootstraper.EyetrackerManager;

            _interactor = eyetrackerManager.WpfInteractorAgent.AddInteractorFor(this);

            _stream = _interactor.GetGazePointDataStream(Tobii.Interaction.Framework.GazePointDataMode.LightlyFiltered);
            _stream.Next += Stream_Next;
        }

        private void Stream_Next(object sender, StreamData<GazePointData> e)
        {
            _lastY = e.Data.Y;
            _lastX = e.Data.X;
        }

        private void ZoomIn(bool zoomIn)
        {
            var bounds = _interactor.GetBounds();
            var relativeX = _lastX - (bounds.X);
            var relativeY = _lastY - (bounds.Y);

            GazeVisualizationControl.Margin = new Thickness(relativeX - GazeVisualizationControl.Width / 2, relativeY - GazeVisualizationControl.Height / 2, 0, 0);

            AnimateGazeZoomVisualization();
            ZoomIn(relativeX, relativeY, ZoomFactor);
        }

        private void ZoomBorder_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= ZoomBorder_Unloaded;
            MouseWheel -= Child_MouseWheel;
            MouseLeftButtonDown -= Child_MouseLeftButtonDown;
            MouseLeftButtonUp -= Child_MouseLeftButtonUp;
            MouseMove -= Child_MouseMove;
            PreviewMouseRightButtonDown -= Child_PreviewMouseRightButtonDown;
            _messenger.Unsubscribe(_prepareNewImageSubscriptionId);
            _messenger.Unsubscribe(_rotateLeftSubscriptionId);
            _messenger.Unsubscribe(_zoomSubscriptionId);
            _stream.Next -= Stream_Next;

            Bootstraper.EyetrackerManager.WpfInteractorAgent.RemoveInteractor(this);
        }

        private void Rotate()
        {
            if (_child != null)
            {
                var rt = GetRotateTransform(_child);
                if (rt.Angle == -360)
                {
                    rt.Angle = 0;
                }

                var to = rt.Angle;
                to -= 90;

                AnimateRotateTransform(rt, to, 400);
            }
        }

        public void Reset(bool quickReset)
        {
            if (_child != null)
            {
                // reset zoom
                var st = GetScaleTransform(_child);
                AnimateScaleTransform(st, 1.0, ZoomAndTranslateAnimationDurationInMs);

                // reset pan
                var tt = GetTranslateTransform(_child);
                AnimateTranslateTransform(tt, 0.0, 0.0, quickReset ? QuickAnimationDurationInMs : ZoomAndTranslateAnimationDurationInMs);

                var rt = GetRotateTransform(_child);
                AnimateRotateTransform(rt, 0, QuickAnimationDurationInMs);
            }
        }

        private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_child != null)
            {
                var st = GetScaleTransform(_child);
                st.CenterX = -(_child.RenderSize.Width / 2);
                st.CenterY = -(_child.RenderSize.Height / 2);

                var tt = GetTranslateTransform(_child);

                bool incrementZoom = e.Delta > 0;

                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(_child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                var newScale = incrementZoom ? st.ScaleX * ZoomFactor : st.ScaleX / ZoomFactor;
                AnimateScaleTransform(st, newScale, ZoomAndTranslateAnimationDurationInMs);
                AnimateTranslateTransform(tt, abosuluteX - relative.X * newScale, abosuluteY - relative.Y * newScale, ZoomAndTranslateAnimationDurationInMs);
            }
        }

        private void AnimateGazeZoomVisualization()
        {
            if (_disappearAnimation == null)
            {
                _disappearAnimation = new DoubleAnimation(1.0, 0, TimeSpan.FromMilliseconds(500))
                {
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
                };
            }

            var st = (ScaleTransform)GazeVisualizationControl.RenderTransform;

            _disappearAnimation.Completed += DisappearAnimation_Completed;

            GazeVisualizationControl.BeginAnimation(Grid.OpacityProperty, _disappearAnimation);

            AnimateScaleTransform(st, 1.3, 300, true);
        }

        private void DisappearAnimation_Completed(object sender, EventArgs e)
        {
            var st = (ScaleTransform)GazeVisualizationControl.RenderTransform;
            st.ScaleX = 0.8; st.ScaleY = 0.8;
            _disappearAnimation.Completed -= DisappearAnimation_Completed;
        }

        private void AnimateRotateTransform(RotateTransform rt, double to, int durationInMs)
        {
            var animation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(durationInMs))
            {
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
            };
            rt.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void AnimateScaleTransform(ScaleTransform st, double to, int durationInMs, bool stop = false)
        {
            var animation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(durationInMs))
            {
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut },
                FillBehavior = stop ? FillBehavior.Stop : FillBehavior.HoldEnd
            };
            st.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private void AnimateTranslateTransform(TranslateTransform tt, double toX, double toY, int durationInMs)
        {
            if (_translateAvailable == true)
            {
                _translateAvailable = false;
                _animationX = new DoubleAnimation(toX, TimeSpan.FromMilliseconds(durationInMs))
                {
                    EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
                };

                var animationY = new DoubleAnimation(toY, TimeSpan.FromMilliseconds(durationInMs))
                {
                    EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
                };

                _animationX.Completed += _animationX_Completed;

                tt.BeginAnimation(TranslateTransform.XProperty, _animationX, HandoffBehavior.Compose);

                tt.BeginAnimation(TranslateTransform.YProperty, animationY, HandoffBehavior.Compose);
            }
        }

        private void _animationX_Completed(object sender, EventArgs e)
        {
            _animationX.Completed -= _animationX_Completed;
            _translateAvailable = true;
        }

        private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                if (e.ClickCount == 1)
                {
                    var tt = GetTranslateTransform(_child);
                    _start = e.GetPosition(this);
                    _origin = new Point(tt.X, tt.Y);
                    // Cursor = Cursors.Hand;
                    _child.CaptureMouse();
                }
                else
                {
                    Point relative = e.GetPosition(_child);
                    ZoomIn(relative.X, relative.Y, 2);
                }
            }
        }

        private void ZoomIn(double relativeX, double relativeY, double zoomScale)
        {
            var st = GetScaleTransform(_child);
            st.CenterX = -(_child.RenderSize.Width / 2);
            st.CenterY = -(_child.RenderSize.Height / 2);

            var tt = GetTranslateTransform(_child);

            double abosuluteX;
            double abosuluteY;

            abosuluteX = relativeX * st.ScaleX + tt.X;
            abosuluteY = relativeY * st.ScaleY + tt.Y;

            var newScale = st.ScaleX * zoomScale;

            AnimateScaleTransform(st, newScale, ZoomAndTranslateAnimationDurationInMs);
            AnimateTranslateTransform(tt, abosuluteX - relativeX * newScale, abosuluteY - relativeY * newScale, ZoomAndTranslateAnimationDurationInMs);
        }

        private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                _child.ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
        }

        private void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Reset(false);
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (_child != null)
            {
                if (_child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(_child);
                    var rt = GetRotateTransform(_child);
                    
                    Vector v = _start - e.GetPosition(this);
                    
                    var result = Rotate(v, -rt.Angle);

                    AnimateTranslateTransform(tt, _origin.X - result.X, _origin.Y - result.Y, QuickAnimationDurationInMs);
                }
            }
        }

        public static Vector Rotate(Vector v, double degrees)
        {
            var radians = degrees * Math.PI / 180;
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new Vector(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }
    }
}
