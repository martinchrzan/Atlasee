using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VisFileManager.Helpers
{
    public static class ApplicationInfoProvider
    {
        public static double GetApplicationContentHeight()
        {
            return GetApplicationMainArea().ActualHeight;
        }

        public static double GetApplicationTop()
        {
            return Application.Current.MainWindow.Top;
        }

        public static string GetApplicationExecutableDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static Grid GetApplicationMainArea()
        {
            return ((Grid)Application.Current.MainWindow.Content);
        }
    }
}
