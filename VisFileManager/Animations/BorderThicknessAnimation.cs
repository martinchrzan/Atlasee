using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace VisFileManager.Animations
{
    public class BorderThicknessAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(Thickness);

        protected override Freezable CreateInstanceCore()
        {
            return new BorderThicknessAnimation();
        }

        static BorderThicknessAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(Thickness),
                typeof(BorderThicknessAnimation));

            ToProperty = DependencyProperty.Register("To", typeof(Thickness),
                typeof(BorderThicknessAnimation));
        }

        public static readonly DependencyProperty FromProperty;
        public Thickness From
        {
            get
            {
                return (Thickness)GetValue(FromProperty);
            }
            set
            {
                SetValue(FromProperty, value);
            }
        }
        public static readonly DependencyProperty ToProperty;
        public Thickness To
        {
            get
            {
                return (Thickness)GetValue(ToProperty);
            }
            set
            {
                SetValue(ToProperty, value);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = ((Thickness)GetValue(FromProperty)).Bottom;
            double toVal = ((Thickness)GetValue(ToProperty)).Bottom;

            if (fromVal > toVal)
            {
                var newValue = (1 - animationClock.CurrentProgress.Value) *
                    (fromVal - toVal) + toVal;
                return new Thickness(newValue, newValue, newValue, newValue);
            }
            else
            {
                var newValue = animationClock.CurrentProgress.Value *
                    (toVal - fromVal) + fromVal;
                return new Thickness(newValue, newValue, newValue, newValue);
            }
        }
    }
}
