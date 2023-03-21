using System.Windows;
using System.Windows.Controls;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.DataTemplateSelectors
{
    public class DetailsActionItemDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item is ISimpleDetailsItemViewModel)
            {
                return ((FrameworkElement)container).FindResource("simpleDetailsItemView")
                     as DataTemplate;
            }
            else if(item is IComplexDetailsItemViewModel)
            {
                return ((FrameworkElement)container).FindResource("complexDetailsItemView")
                     as DataTemplate;
            }
            
            return base.SelectTemplate(item, container);
        }
    }
}
