using System;
using System.ComponentModel.Composition;
using VisFileManager.Controls;
using VisFileManager.Helpers;
using VisFileManager.Shared;
using VisFileManager.ViewModelContracts;

namespace VisFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CustomMainWindow
    {
        public MainWindow()
        {
            Closing += MainWindow_Closing;
            Bootstraper.ApplicationInstanceId = Guid.NewGuid();
            Bootstraper.InitializeContainer(this);
            MigrateUserSettings();
            InitializeComponent();
            DataContext = this;

            Logger.LogInfo("Initialized");
        }

        private void MigrateUserSettings()
        {
            if (Properties.Settings.Default.UpdateSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
                Properties.Settings.Default.Save();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= MainWindow_Closing;
            Bootstraper.Dispose();
        }

        [Import]
        public IMainViewModel MainViewModel { get; set; }
    }
}
