using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using System.Windows;
using VisFileManager.Shared;

namespace VisFileManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string Message = "Unhandled exception";

        [STAThread]
        public static void Main()
        {
            ProfileOptimization.SetProfileRoot(Path.GetTempPath());
            ProfileOptimization.StartProfile("AtlaseeOptimizationProfile");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                var ex = e.ExceptionObject as Exception;
                Logger.LogError(Message, ex);
            }
        }
    }
}
