using System.Collections.ObjectModel;
using System.Windows.Input;
using VisFileManager.Helpers;

namespace VisFileManager.ViewModelContracts
{
    public interface ISettingsViewModel
    {
        ICommand OpenSettingsCommand { get; }

        bool ShowHiddenItems { get; set; }

        bool ShowDeleteConfirmationDialog { get; set; }

        bool EyetrackingSettingsVisible { get; }

        bool EyetrackingEnabled { get; set; }

        string PrimaryKey { get; }

        string SecondaryKey { get; }

        string ScrollKey { get; }

        uint SlideshowDurationInSec { get; }

        ICommand ChangePrimaryKeyCommand { get; }

        ICommand ChangeSecondaryKeyCommand { get; }

        ICommand ChangeScrollKeyCommand { get; }

        bool SetButtonOverlayVisible { get; }

        string ApplicationVersion { get; }

        Background SelectedBackground { get; set; }

        ObservableCollection<Background> AvailableBackgrounds { get; }
    }
}
