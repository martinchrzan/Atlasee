using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Tobii.Interaction.Wpf;

namespace VisFileManager.Controls
{
    public class GazeActivableButton : Button
    {
        private DependencyPropertyDescriptor _hasGazeDescriptor;

        public bool HasGaze
        {
            get { return (bool)GetValue(HasGazeProperty); }
            set { SetValue(HasGazeProperty, value); }
        }

        public static readonly DependencyProperty HasGazeProperty = DependencyProperty.Register(
            "HasGaze", typeof(bool), typeof(GazeActivableButton));

        public GazeActivableButton()
        {
            Loaded += GazeActivableButton_Loaded;
            Unloaded += GazeActivableButton_Unloaded;
        }

        private void GazeActivableButton_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetIsActivatable(true);
            this.SetIsTentativeFocusEnabled(true);
            AttachHasGazeDescriptor();
        }

        private void AttachHasGazeDescriptor()
        {
            if (_hasGazeDescriptor == null)
            {
                _hasGazeDescriptor = DependencyPropertyDescriptor.FromProperty(Tobii.Interaction.Wpf.Behaviors.HasGazeProperty, typeof(GazeActivableButton));
            }
            _hasGazeDescriptor.AddValueChanged(this, HasGazeChanged);
        }

        private void HasGazeChanged(object sender, EventArgs e)
        {
            HasGaze = this.GetHasGaze();
        }

        private void GazeActivableButton_Unloaded(object sender, RoutedEventArgs e)
        {
            _hasGazeDescriptor.RemoveValueChanged(this, HasGazeChanged);
            this.SetIsActivatable(false);
            this.SetIsTentativeFocusEnabled(false);
        }
    }
}

