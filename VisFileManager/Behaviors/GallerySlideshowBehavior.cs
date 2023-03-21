using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VisFileManager.Animations;
using VisFileManager.Controls;
using VisFileManager.Messenger;

namespace VisFileManager.Behaviors
{
    public class GallerySlideshowBehavior : Behavior<Grid>
    {
        public Grid MainGrid
        {
            get { return (Grid)GetValue(MainGridProperty); }
            set { SetValue(MainGridProperty, value); }
        }

        public static readonly DependencyProperty MainGridProperty = DependencyProperty.Register(
            "MainGrid", typeof(Grid), typeof(GallerySlideshowBehavior));

        public GazeActivableButton RotateButton
        {
            get { return (GazeActivableButton)GetValue(RotateButtonProperty); }
            set { SetValue(RotateButtonProperty, value); }
        }

        public static readonly DependencyProperty RotateButtonProperty = DependencyProperty.Register(
            "RotateButton", typeof(GazeActivableButton), typeof(GallerySlideshowBehavior));

        public GazeActivableButton SlideshowButton
        {
            get { return (GazeActivableButton)GetValue(SlideshowButtonProperty); }
            set { SetValue(SlideshowButtonProperty, value); }
        }

        public static readonly DependencyProperty SlideshowButtonProperty = DependencyProperty.Register(
            "SlideshowButton", typeof(GazeActivableButton), typeof(GallerySlideshowBehavior));

        protected override void OnAttached()
        {
            var messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            messenger.Subscribe<bool>(MessageIds.GallerySlideshowMode, (message) =>
            {
                var brushAnimation = new BrushAnimation();
                brushAnimation.To = Brushes.Black;
                brushAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(800));

                GridLengthAnimation heightAnimation = new GridLengthAnimation();
                heightAnimation.To = new GridLength(0);
                heightAnimation.From = new GridLength(120.0);
                heightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                GridLengthAnimation heightAnimation2 = new GridLengthAnimation();
                heightAnimation2.To = new GridLength(0);
                heightAnimation2.From = new GridLength(35.0);
                heightAnimation2.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                heightAnimation.DecelerationRatio = 0.5;
                heightAnimation2.DecelerationRatio = 0.5;

                MainGrid.RowDefinitions[MainGrid.RowDefinitions.Count - 1].BeginAnimation(RowDefinition.HeightProperty, heightAnimation);
                MainGrid.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, heightAnimation2);

                MainGrid.BeginAnimation(Grid.BackgroundProperty, brushAnimation);

                RotateButton.Visibility = Visibility.Collapsed;
                SlideshowButton.Visibility = Visibility.Collapsed;
            });
        }
    }
}
