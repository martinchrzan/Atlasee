using System.Windows;
using System.Windows.Controls;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.DataTemplateSelectors
{
    public class GridViewItemDataTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _fileItemViewGridStyle;
        private DataTemplate _driveItemViewGridStyle;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IFileItemViewModel)
            {
                if (_fileItemViewGridStyle == null)
                {
                    _fileItemViewGridStyle = ((FrameworkElement)container).FindResource("fileItemViewGridStyle") as DataTemplate;
                }
                return _fileItemViewGridStyle;
            }
            else if (item is IDriveItemViewModel)
            {
                if (_driveItemViewGridStyle == null)
                {
                    _driveItemViewGridStyle = ((FrameworkElement)container).FindResource("driveItemViewGridStyle") as DataTemplate;
                }
                return _driveItemViewGridStyle;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
