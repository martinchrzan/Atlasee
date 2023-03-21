using IOExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.FileSystemHelpers
{
    [Export(typeof(IClipboardManager))]
    public class ClipboardManager : ViewModelBase, IClipboardManager
    {
        private const int  DelayBeforeShowingProgressBarInMs = 500;
        private bool _canPaste = false;

        private bool _isFinished;
        private readonly ICopyProgressBottomPanelViewModel _progressBarPanelViewModel;
        private readonly IGlobalFileManager _globalFileManager;
        private readonly IDispatcherTaskScheduler _dispatcherTaskScheduler;
        private readonly IFileOperationsManager _fileOperationsManager;
        private readonly IMessenger _messenger;

        [ImportingConstructor]
        public ClipboardManager(ICopyProgressBottomPanelViewModel progressBarPanelViewModel, IGlobalFileManager globalFileManager, IDispatcherTaskScheduler dispatcherTaskScheduler, IFileOperationsManager fileOperationsManager,
            IMessenger messenger)
        {
            _progressBarPanelViewModel = progressBarPanelViewModel;
            _globalFileManager = globalFileManager;
            _dispatcherTaskScheduler = dispatcherTaskScheduler;
            _fileOperationsManager = fileOperationsManager;
            _messenger = messenger;

            _messenger.Subscribe<DragAndDropMessage>(MessageIds.DragAndDropMessage, DragAndDropCalled);

            ApplicationCommands.Paste.CanExecuteChanged += Paste_CanExecuteChanged;
            FillPasteItemsArray();
        }

        private void DragAndDropCalled(DragAndDropMessage message)
        {
            Task.Run(async () =>
            {
                await Paste(message.ItemsToDrop.ToList(), message.IsCopyOperation);
            });
        }

        public event EventHandler CanPasteChanged;

        public void Copy(IEnumerable<FormattedPath> paths)
        {
            AddIntoClipboard(paths, DragDropEffects.Copy);
        }

        public void Copy(FormattedPath path)
        {
            Copy(new[] { path });
        }

        public void Cut(FormattedPath path)
        {
            Cut(new[] { path });
        }

        public void Cut(IEnumerable<FormattedPath> paths)
        {
            AddIntoClipboard(paths, DragDropEffects.Move);
        }

        public async void Paste()
        {
            await Paste(PasteItems.ToList(), !IsCut);
        }

        private async Task Paste(List<string> pasteItems, bool isCopy)
        {
            _isFinished = false;

            var multipleItems = pasteItems.Count > 1;

            var cancellationToken = new CancellationTokenSource();
            _progressBarPanelViewModel.Prepare(multipleItems, cancellationToken);

            _progressBarPanelViewModel.OverallProgressCurrentFile = "Preparing...";

            _dispatcherTaskScheduler.ScheduleAction(ShouldShowProgressBar, DelayBeforeShowingProgressBarInMs);

            var targetPath = _globalFileManager.CurrentPath.Path;

            if (!isCopy)
            {
                await _fileOperationsManager.Move(pasteItems.Select(it => FormattedPath.CreateFormattedPath(it)), _globalFileManager.CurrentPath, cancellationToken, (total, partial, speed, currentItem) =>
                {
                    HandleProgressReport(total, partial, speed, currentItem, multipleItems, pasteItems);
                });
            }
            else
            {
                await _fileOperationsManager.Copy(pasteItems.Select(it => FormattedPath.CreateFormattedPath(it)), _globalFileManager.CurrentPath, cancellationToken, (total, partial, speed, currentItem) =>
                {
                    HandleProgressReport(total, partial, speed, currentItem, multipleItems, pasteItems);
                });
            }

            _isFinished = true;
            _progressBarPanelViewModel.Finished();
        }

        private void HandleProgressReport(double total, double partial, string speed, string currentItem, bool multipleItems, List<string> pasteItems)
        {
            if (multipleItems)
            {
                var index = pasteItems.IndexOf(currentItem) + 1;
                _progressBarPanelViewModel.OverallProgressCurrentFile = "Copying " + index + " out of " + pasteItems.Count + " items";
                _progressBarPanelViewModel.PartialProgressCurrentFile = Path.GetFileName(currentItem);
                _progressBarPanelViewModel.PartialProgressPercentage = partial;
                _progressBarPanelViewModel.OverallProgressPercentage = total;
            }
            else
            {
                _progressBarPanelViewModel.OverallProgressCurrentFile = Path.GetFileName(currentItem);
                _progressBarPanelViewModel.OverallProgressPercentage = partial;
            }
            _progressBarPanelViewModel.CurrentTransferSpeed = speed;   
        }
        
        public bool CanPaste
        {
            get { return _canPaste; }
            set
            {
                _canPaste = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> PasteItems { get; } = new ObservableCollection<string>();

        public bool IsCut { get; private set; }

        private void Paste_CanExecuteChanged(object sender, EventArgs e)
        {
            if(CanPaste != Clipboard.ContainsFileDropList())
            {
                CanPaste = Clipboard.ContainsFileDropList();
              
                RaiseCanPasteChanged();
            }

            FillPasteItemsArray();
        }

        private void FillPasteItemsArray()
        {
            if(Clipboard.ContainsFileDropList())
            {
                IsCut = IsCutActive();
            }
            
            PasteItems.Clear();
            foreach (var item in Clipboard.GetFileDropList())
            {
                PasteItems.Add(item);
            }
        }
        
        private void ShouldShowProgressBar()
        {
            if (!_isFinished)
            {
                _progressBarPanelViewModel.Visible = true;
            }
        }

        private void RaiseCanPasteChanged()
        {
            CanPasteChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool IsCutActive()
        {
            var data = Clipboard.GetData("Preferred Dropeffect");
            if (data == null)
            {
                return false;
            }

            var e = BitConverter.ToInt32(((MemoryStream)data).ToArray(), 0);
            DragDropEffects effect = (DragDropEffects)e;

            return effect == DragDropEffects.Move;
        }

        private void AddIntoClipboard(IEnumerable<FormattedPath> paths, DragDropEffects effect)
        {
            var droplist = new StringCollection();
            droplist.AddRange(paths.Select(x => x.Path).ToArray());

            var data = new DataObject();
            data.SetFileDropList(droplist);
            data.SetData("Preferred Dropeffect", new MemoryStream(BitConverter.GetBytes((int)effect)));
            Clipboard.SetDataObject(data);
        }
    }
}
