using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VisFileManager.Converters
{
    // Visible based on number of items
    // parameter bool - true - visible if any items, collapsed otherwise
    //                - false - collapsed if any items, visible otherwise
    // 
    public class NumberOfItemsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var anyItems = value != null && ((ICollection)value).Count > 0;
            if(parameter == null || string.IsNullOrEmpty((string)parameter))
            {
                throw new Exception("Parameter has to specify if object should be visible or not based on number of items");
            }

            var param = bool.Parse(parameter.ToString());

            if (param && anyItems)
            {
                return Visibility.Visible;
            }
            else if (!param && anyItems || (param && !anyItems))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
