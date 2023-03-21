using System.Windows;
using System.Windows.Controls;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.DataTemplateSelectors
{
    public class ListViewItemDataTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _fileItemViewListStyle;
        private DataTemplate _driveItemViewListStyle;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IFileItemViewModel)
            {
                if (_fileItemViewListStyle == null)
                {
                    _fileItemViewListStyle = ((FrameworkElement)container).FindResource("fileItemViewListStyle") as DataTemplate;
                }
                return _fileItemViewListStyle;
            }
            else if (item is IDriveItemViewModel)
            {
                if (_driveItemViewListStyle == null)
                {
                    _driveItemViewListStyle = ((FrameworkElement)container).FindResource("driveItemViewListStyle") as DataTemplate;
                }
                return _driveItemViewListStyle;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
