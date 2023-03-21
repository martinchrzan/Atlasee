using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using VisFileManager.Helpers;

namespace VisFileManager.Behaviors
{
    public class ChangeBackgroundBehavior : Behavior<UserControl>
    {
        private IBackgroundManager _backgroundManager;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _backgroundManager = Bootstraper.Container.GetExportedValue<IBackgroundManager>();
            SetBackground(_backgroundManager.SelectedBackground);
            _backgroundManager.OnSelectedBackgroundChanged += BackgroundManager_OnSelectedBackgroundChanged;
        }

        private void BackgroundManager_OnSelectedBackgroundChanged(object sender, EventArgs e)
        {
            SetBackground(_backgroundManager.SelectedBackground);
        }

        private void SetBackground(Background background)
        {
            AssociatedObject.Background = background.Brush;
        }
    }
}
