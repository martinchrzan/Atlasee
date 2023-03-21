using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.Messenger;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.Helpers
{
    [Export(typeof(IDialogHelper))]
    public class DialogHelper : IDialogHelper
    {
        private AutoResetEvent _actionConfirmed = new AutoResetEvent(false);
        private GalleryViewWindow _galleryWindow;
        private readonly IThreeButtonsDialogViewModel _threeButtonsDialogViewModel;
        private readonly IErrorDialogViewModel _errorDialogViewModel;

        private bool? Result { get; set; }

        [ImportingConstructor]
        public DialogHelper(IThreeButtonsDialogViewModel threeButtonsDialogViewModel, IErrorDialogViewModel errorDialogViewModel, IMessenger messenger)
        {
            _threeButtonsDialogViewModel = threeButtonsDialogViewModel;
            _errorDialogViewModel = errorDialogViewModel;

            messenger.Subscribe<bool>(MessageIds.GalleryCloseWindow, CloseGalleryWindow);
        }

        public bool? ShowThreeButtonsDialog(string message, string firstButtonCaption, string secondButtonCaption, string cancelButtonCaption = "Cancel")
        {
            return ShowThreeButtonsDialog(message, () => { }, firstButtonCaption, () => { }, secondButtonCaption, new CancellationTokenSource(), cancelButtonCaption);
        }

        public bool? ShowTwoButtonsDialog(string message, Action firstButtonAction, string firstButtonCaption, CancellationTokenSource cancellationTokenSource, string cancelButtonCaption = "Cancel")
        {
            return ShowThreeButtonsDialog(message, firstButtonAction, firstButtonCaption, null, string.Empty,  new CancellationTokenSource(), cancelButtonCaption);
        }

        public bool? ShowThreeButtonsDialog(string message, Action firstButtonAction, string firstButtonCaption, Action secondButtonAction, string secondButtonCaption, CancellationTokenSource cancellationTokenSource, string cancelButtonCaption = "Cancel")
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            bool? result = false;
            App.Current.Dispatcher.Invoke(() =>
            {
                var cancelled = false;
                var window = GetDialogWindow(false);
                window.Content = _threeButtonsDialogViewModel;

                cancellationTokenSource.Token.Register(() =>
                {
                    window.Close();
                    cancelled = true;
                });

                _threeButtonsDialogViewModel.ShowDialog(message, () => { firstButtonAction.Invoke(); window.DialogResult = true; }, firstButtonCaption, () => { secondButtonAction.Invoke(); window.DialogResult = false; }, secondButtonCaption, cancellationTokenSource);

                result = window.ShowDialog();
                if (cancelled)
                {
                    result= null;
                }
                autoResetEvent.Set();
            });
            autoResetEvent.WaitOne();

            return result;
        }

        public void ShowError(string message)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            App.Current.Dispatcher.Invoke(() =>
            {
                var window = GetDialogWindow(false);
                window.Content = _errorDialogViewModel;

                _errorDialogViewModel.ShowError(message, () => window.DialogResult = true);

                window.ShowDialog();
                autoResetEvent.Set();
            });
            autoResetEvent.WaitOne();
        }

        public void ShowDialog(ViewModelBase content, bool fullscreen)
        {
            var window = GetDialogWindow(true);
            window.Content = content;
            window.ShowDialog();
        }


        public void ShowGalleryWindow(ViewModelBase content)
        {
            _galleryWindow = new GalleryViewWindow();
            _galleryWindow.Owner = Application.Current.MainWindow;
            _galleryWindow.Content = content;
            _galleryWindow.ShowDialog();
            content.Unsubscribe();
        }

        private void CloseGalleryWindow(bool obj)
        {
            if(_galleryWindow != null)
            {
                _galleryWindow.DialogResult = true;
                _galleryWindow.Close();
                _galleryWindow = null;
            }
        }

        private Window GetDialogWindow(bool fullScreen)
        {
            var window = new Window
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                ShowInTaskbar = false,
                WindowState = fullScreen ? WindowState.Maximized : WindowState.Normal,
                Background = System.Windows.Media.Brushes.Transparent,
            };

            if(fullScreen)
            {
                window.ResizeMode = ResizeMode.NoResize;
            }

            return window;
        }
    }
}
