using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VisFileManager.Shared;

namespace VisFileManager.Converters
{
    /// <summary>
    /// Converter to convert percentage value into fraction of maximum
    /// first value is expected to be current progress percentage
    /// second value is expected to be maximum value it should reach when 100%
    /// converter parameter is used when first value is in percentage not in 0-1 range - use 0.01 as parameter then
    /// </summary>
    public class PercentageToMaxValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values == null) return null;

                double currentProgress = ((double)values[0]);
                double max = ((double)values[1]);

                double multiplicationFactor = 1;
                if (parameter != null)
                {
                    multiplicationFactor = double.Parse((string)parameter, CultureInfo.InvariantCulture);
                }

                // multiplication factor can be 0.01 because current progress is in percentages not in 0<x>1 range
                return (currentProgress * multiplicationFactor) * max;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
