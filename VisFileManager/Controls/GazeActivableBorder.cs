using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Tobii.Interaction.Wpf;

namespace VisFileManager.Controls
{
    public class GazeActivableBorder : Border
    {
        private DependencyPropertyDescriptor _hasGazeDescriptor;

        public bool HasGaze
        {
            get { return (bool)GetValue(HasGazeProperty); }
            set { SetValue(HasGazeProperty, value); }
        }

        public event EventHandler GazeChanged;

        public static readonly DependencyProperty HasGazeProperty = DependencyProperty.Register(
            "HasGaze", typeof(bool), typeof(GazeActivableBorder));

        public GazeActivableBorder()
        {
            Loaded += GazeActivableBorder_Loaded;
            LayoutUpdated += GazeActivableBorder_LayoutUpdated;
        }

        private void GazeActivableBorder_LayoutUpdated(object sender, EventArgs e)
        {
            if (IsVisible == false && _hasGazeDescriptor != null)
            {
                _hasGazeDescriptor.RemoveValueChanged(this, HasGazeChanged);

                this.SetIsActivatable(false);
                this.SetIsTentativeFocusEnabled(false);

                Bootstraper.EyetrackerManager.WpfInteractorAgent.RemoveInteractor(this);
            }
        }

        private void GazeActivableBorder_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetIsActivatable(true);
            this.SetIsTentativeFocusEnabled(true);

            AttachHasGazeDescriptor();
        }

        private void AttachHasGazeDescriptor()
        {
            if (_hasGazeDescriptor == null)
            {
                _hasGazeDescriptor = DependencyPropertyDescriptor.FromProperty(Tobii.Interaction.Wpf.Behaviors.HasGazeProperty, typeof(GazeActivableBorder));
            }
            _hasGazeDescriptor.AddValueChanged(this, HasGazeChanged);
        }

        private void HasGazeChanged(object sender, EventArgs e)
        {
            HasGaze = this.GetHasGaze();
            GazeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

