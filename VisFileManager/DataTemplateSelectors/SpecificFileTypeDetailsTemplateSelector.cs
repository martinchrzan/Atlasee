using System.Windows;
using System.Windows.Controls;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.DataTemplateSelectors
{
    public class SpecificFileTypeDetailsTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IFileDetailsSpecificImageViewModel)
            {
                return ((FrameworkElement)container).FindResource("imageDetailsView")
                    as DataTemplate;
            }
            if (item is IFileDetailsSpecificVideoViewModel)
            {
                return ((FrameworkElement)container).FindResource("videoDetailsView")
                    as DataTemplate;
            }
            if(item is IFileDetailsSpecificAudioViewModel)
            {
                return ((FrameworkElement)container).FindResource("audioDetailsView")
                    as DataTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
