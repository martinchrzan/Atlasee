using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace VisFileManager.Behaviors
{
    public class ListBoxFocusBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;

            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FocusSelectedItem();
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Focus();
            FocusSelectedItem();
        }

        private void FocusSelectedItem()
        {
            var listBoxItem = (ListBoxItem)AssociatedObject.ItemContainerGenerator.ContainerFromItem(AssociatedObject.SelectedItem);
            if (listBoxItem != null)
            {
                listBoxItem.Focus();
            }
        }
    }
}
