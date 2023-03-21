using Microsoft.Xaml.Behaviors;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;

namespace VisFileManager.Behaviors
{
    public class DragAndDropBehavior : Behavior<ListBox>
    {
        private const string CopyHereText = "Copy here";
        private const string MoveHereText = "Move here";
        private IMessenger _messenger;
        private IGlobalFileManager _globalFileManager;

        public Border DropEffectBorder
        {
            get
            {
                return (Border)GetValue(DropEffectBorderProperty);
            }
            set
            {
                SetValue(DropEffectBorderProperty, value);
            }
        }

        public static DependencyProperty DropEffectBorderProperty = DependencyProperty.Register("DropEffectBorder", typeof(Border), typeof(DragAndDropBehavior));

        public TextBlock DescriptionTextBlock
        {
            get
            {
                return (TextBlock)GetValue(DescriptionTextBlockProperty);
            }
            set
            {
                SetValue(DescriptionTextBlockProperty, value);
            }
        }

        public static DependencyProperty DescriptionTextBlockProperty = DependencyProperty.Register("DescriptionTextBlock", typeof(TextBlock), typeof(DragAndDropBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.PreviewDragEnter -= AssociatedObject_PreviewDragEnter;
            AssociatedObject.PreviewDragLeave -= AssociatedObject_PreviewDragLeave;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Drop += AssociatedObject_Drop;
            AssociatedObject.PreviewDragEnter += AssociatedObject_PreviewDragEnter;
            AssociatedObject.PreviewDragLeave += AssociatedObject_PreviewDragLeave;
            _messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            _globalFileManager = Bootstraper.Container.GetExportedValue<IGlobalFileManager>();
        }

        private void AssociatedObject_PreviewDragLeave(object sender, DragEventArgs e)
        {
            HideBorderEffect();
        }

        private void AssociatedObject_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                AppearEffectAnimate(ShouldCopy(e));
            }
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var message = new DragAndDropMessage(ShouldCopy(e), (string[])e.Data.GetData(DataFormats.FileDrop));
                _messenger.Send(MessageIds.DragAndDropMessage, message);

                HideBorderEffect();
            }
        }

        private void HideBorderEffect()
        {
            DoubleAnimation opacity = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(300)));
            opacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut };

            DoubleAnimation opacity2 = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(300)));
            opacity2.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut };


            DropEffectBorder.BeginAnimation(Border.OpacityProperty, opacity);
            DescriptionTextBlock.BeginAnimation(TextBlock.OpacityProperty, opacity2);
        }

        private void AppearEffectAnimate(bool isCopy)
        {
            if (isCopy)
            {
                DescriptionTextBlock.Text = CopyHereText;
            }
            else
            {
                DescriptionTextBlock.Text = MoveHereText;
            }

            var _opacityAnimation = new DoubleAnimation(0.6, new Duration(TimeSpan.FromMilliseconds(300)));
            _opacityAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut };

            DoubleAnimation opacity2 = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(300)));
            opacity2.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut };

            DropEffectBorder.BeginAnimation(Border.OpacityProperty, _opacityAnimation);
            DescriptionTextBlock.BeginAnimation(TextBlock.OpacityProperty, opacity2);
        }

        private bool ShouldCopy(DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Windows logic
            var copy = true;

            // shift and control keys have priority
            if (e.KeyStates.HasFlag(DragDropKeyStates.ShiftKey))
            {
                copy = false;
            }
            else if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
            {
                copy = true;
            }
            else
            {
                var destination = _globalFileManager.CurrentPath.Path;

                // enough to check the first one as you can move/copy files just from one location
                if (Path.GetPathRoot(files[0]) == Path.GetPathRoot(destination))
                {
                    if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
                    {
                        // move if within the same drive
                        copy = false;
                    }
                }
                else
                {
                    copy = false;
                }
            }
            return copy;
        }
    }
}
