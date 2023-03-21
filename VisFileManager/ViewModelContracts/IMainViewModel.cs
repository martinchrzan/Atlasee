using System;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModelContracts
{
    public interface IMainViewModel
    {
        ICopyProgressBottomPanelViewModel ProgressBarPanelViewModel { get; }

        IDetailsViewModel DetailsViewModel { get; }

        ISettingsViewModel SettingsViewModel { get; }

        ISidePanelViewModel SidePanelViewModel { get; }

        ITopPanelViewModel TopPanelViewModel { get; }

        IMainFileViewModel MainFileViewModel { get; }

        IThreeButtonsDialogViewModel ThreeButtonsDialogViewModel { get; }

        IErrorDialogViewModel ErrorDialogViewModel { get; }
    }
}
