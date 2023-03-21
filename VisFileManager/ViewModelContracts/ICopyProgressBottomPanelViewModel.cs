using System.Threading;
using System.Windows.Input;

namespace VisFileManager.ViewModelContracts
{
    public interface ICopyProgressBottomPanelViewModel
    {
        bool MultipleFiles { get; set; }

        bool Visible { get; set; }

        bool SmallSize { get; }

        double OverallProgressPercentage { get; set; }

        double PartialProgressPercentage { get; set; }

        string CurrentTransferSpeed { get; set; }

        string OverallProgressCurrentFile { get; set; }

        string PartialProgressCurrentFile { get; set; }

        void Prepare(bool multipleFiles, CancellationTokenSource cancellationTokenSource);

        void Finished();

        ICommand CancelCommand { get; }
    }
}
