using System.ComponentModel.Composition;
using VisFileManager.Common;
using VisFileManager.ViewModelContracts;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModels
{
    [Export(typeof(IMainViewModel))]
    public class MainViewModel: ViewModelBase, IMainViewModel
    {
        [ImportingConstructor]
        public MainViewModel(
            ICopyProgressBottomPanelViewModel progressBarPanelViewModel,
            IDetailsViewModel detailsViewModel,
            ISettingsViewModel settingsViewModel,
            ISidePanelViewModel sidePanelViewModel,
            ITopPanelViewModel topPanelViewModel,
            IMainFileViewModel mainFileViewModel,
            IThreeButtonsDialogViewModel threeButtonsDialogViewModel,
            IErrorDialogViewModel errorDialogViewModel)
        {
            ProgressBarPanelViewModel = progressBarPanelViewModel;
            DetailsViewModel = detailsViewModel;
            SettingsViewModel = settingsViewModel;
            SidePanelViewModel = sidePanelViewModel;
            TopPanelViewModel = topPanelViewModel;
            MainFileViewModel = mainFileViewModel;
            ThreeButtonsDialogViewModel = threeButtonsDialogViewModel;
            ErrorDialogViewModel = errorDialogViewModel;
        }

        public ICopyProgressBottomPanelViewModel ProgressBarPanelViewModel { get; }

        public IDetailsViewModel DetailsViewModel { get; }

        public ISettingsViewModel SettingsViewModel { get; }

        public ISidePanelViewModel SidePanelViewModel { get; }

        public ITopPanelViewModel TopPanelViewModel { get; }

        public IMainFileViewModel MainFileViewModel { get; }

        public IThreeButtonsDialogViewModel ThreeButtonsDialogViewModel { get; }

        public IErrorDialogViewModel ErrorDialogViewModel { get; }
    }
}
