using System.Windows;
using System.Windows.Controls;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.DataTemplateSelectors
{
    public class DetailsItemDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        { 
            if (item is IFileDetailsViewModel)
            {
                return ((FrameworkElement)container).FindResource("fileDetailsView")
                     as DataTemplate;
            }
            else if (item is IDirectoryDetailsViewModel)
            {
                return ((FrameworkElement)container).FindResource("directoryDetailsView")
                     as DataTemplate;
            }
            else if(item is IDriveDetailsViewModel)
            {
                return ((FrameworkElement)container).FindResource("driveDetailsView")
                     as DataTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
