using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VisFileManager.Common;

namespace VisFileManager.Helpers
{
    public interface IDialogHelper
    {
        bool? ShowThreeButtonsDialog(string message, string firstButtonCaption, string secondButtonCaption, string cancelButtonCaption = "Cancel");

        bool? ShowTwoButtonsDialog(string message, Action firstButtonAction, string firstButtonCaption, CancellationTokenSource cancellationTokenSource, string cancelButtonCaption = "Cancel");

        bool? ShowThreeButtonsDialog(string message, Action firstButtonAction, string firstButtonCaption, Action secondButtonAction, string secondButtonCaption, CancellationTokenSource cancellationTokenSource, string cancelButtonCaption = "Cancel");

        void ShowError(string message);

        void ShowDialog(ViewModelBase content, bool fullscreen);

        void ShowGalleryWindow(ViewModelBase content);
    }
}
