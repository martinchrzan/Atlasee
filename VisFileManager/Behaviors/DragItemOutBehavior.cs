using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.Behaviors
{
    public class DragItemOutBehavior : Behavior<FrameworkElement>
    {
        private Point _start;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
        }

        private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point mpos = e.GetPosition(null);
            Vector diff = _start - mpos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var listBox = ItemsControlParentProvider.GetMainItemsControl(AssociatedObject) as ListBox;
                if (listBox.SelectedItems.Count == 0)
                {
                    return;
                }

                List<string> files = new List<string>();
                foreach (var item in listBox.SelectedItems)
                {
                    if (item is IFileItemViewModel)
                    {
                        files.Add((item as IFileItemViewModel).FullFormattedPath.Path);
                    }
                }

                if (files.Count > 0)
                {
                    string dataFormat = DataFormats.FileDrop;
                    DataObject dataObject = new DataObject(dataFormat, files.ToArray());
                    DragDrop.DoDragDrop(AssociatedObject, dataObject, DragDropEffects.Copy);
                }
            }
        }

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _start = e.GetPosition(null);


            var listBox = ItemsControlParentProvider.GetMainItemsControl(AssociatedObject) as ListBox;
            // to allow drag of multiple items - if not handled it will select clicked one and remove multiple selection
            // click count to check if we are not doing double click, then we should not handle click but let it pass normaly
            if (listBox.SelectedItems.Contains(AssociatedObject.DataContext) && e.ClickCount == 1)
            {
                e.Handled = true;
                AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            }
        }
    }
}
