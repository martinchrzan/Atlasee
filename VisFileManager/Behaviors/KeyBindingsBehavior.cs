using Microsoft.Xaml.Behaviors;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VisFileManager.FileSystemHelpers;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.Behaviors
{
    public class KeyBindingsBehavior : Behavior<ListBox>
    {
        private IClipboardManager _clipboardManager;
        private IFileOperationsManager _fileOperationsManager;
        private EyetrackerManager _eyetrackerManager;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _clipboardManager = Bootstraper.Container.GetExportedValue<IClipboardManager>();
            _fileOperationsManager = Bootstraper.Container.GetExportedValue<IFileOperationsManager>();
            _eyetrackerManager = Bootstraper.EyetrackerManager;
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (AssociatedObject.SelectedItems != null && AssociatedObject.SelectedItems.Count > 0)
            {
                var files = new List<IFileItemViewModel>();
                foreach (var item in AssociatedObject.SelectedItems)
                {
                    if (item is IFileItemViewModel)
                    {
                        files.Add(item as IFileItemViewModel);
                    }
                }

                if (files.Count == 0)
                {
                    e.Handled = false;
                    return;
                }

                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    // supports keyboard navigation when eyetracking not running, otherwise activation is gaze based
                    if (_eyetrackerManager.EyetrackingAvailability != Tobii.Interaction.Client.EyeXAvailability.Running)
                    {
                        files.First().OpenCommand.Execute(null);
                    }
                }

                else if (e.Key == System.Windows.Input.Key.C && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    _clipboardManager.Copy(files.Select(it => it.FullFormattedPath));
                }
                else if (e.Key == System.Windows.Input.Key.X && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    _clipboardManager.Cut(files.Select(it => it.FullFormattedPath));
                }
                else if (e.Key == System.Windows.Input.Key.Delete)
                {
                    _fileOperationsManager.RemoveItems(files.Select(it => it.FullFormattedPath), new System.Threading.CancellationTokenSource());
                }
            }

            if (e.Key == System.Windows.Input.Key.A && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                AssociatedObject.SelectAll();
            }
            else if (e.Key == System.Windows.Input.Key.V && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                _clipboardManager.Paste();
            }

            e.Handled = false;
        }
    }
}
