using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Globalization;
using System.Windows.Data;

namespace VisFileManager.Converters
{
    public class PathToThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }
            return ShellFile.FromFilePath((string)value).Thumbnail.BitmapSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
