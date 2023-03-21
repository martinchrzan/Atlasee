using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace VisFileManager.Animations
{
    public class CornerRadiusAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(CornerRadius);

        protected override Freezable CreateInstanceCore()
        {
            return new CornerRadiusAnimation();
        }

        static CornerRadiusAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(CornerRadius),
                typeof(CornerRadiusAnimation));

            ToProperty = DependencyProperty.Register("To", typeof(CornerRadius),
                typeof(CornerRadiusAnimation));
        }

        public static readonly DependencyProperty FromProperty;
        public CornerRadius From
        {
            get
            {
                return (CornerRadius)GetValue(FromProperty);
            }
            set
            {
                SetValue(FromProperty, value);
            }
        }
        public static readonly DependencyProperty ToProperty;
        public CornerRadius To
        {
            get
            {
                return (CornerRadius)GetValue(ToProperty);
            }
            set
            {
                SetValue(ToProperty, value);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = ((CornerRadius)GetValue(FromProperty)).BottomLeft;
            double toVal = ((CornerRadius)GetValue(ToProperty)).BottomLeft;

            if (fromVal > toVal)
            {
                var newValue = (1 - animationClock.CurrentProgress.Value) *
                    (fromVal - toVal) + toVal;
                return new CornerRadius(newValue, newValue, newValue, newValue);
            }
            else
            {
                var newValue = animationClock.CurrentProgress.Value *
                    (toVal - fromVal) + fromVal;
                return new CornerRadius(newValue, newValue, newValue, newValue);
            }
        }
    }
}
