using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Validators;

namespace VisFileManager
{
    [Export(typeof(IHistoryManager))]
    public class HistoryManager : ViewModelBase, IHistoryManager
    {
        private readonly HistoryProvider<FormattedPath> _historyProvider = new HistoryProvider<FormattedPath>();
        private readonly IGlobalFileManager _globalFileManager;
        private FormattedPath _previousCurrentPath;
        private FormattedPath _backDirectoryPath;
        private FormattedPath _forwardDirectoryPath;

        [ImportingConstructor]
        public HistoryManager(IGlobalFileManager globalFileManager)
        {
            UndoCommand = new RelayCommand((o) => _historyProvider.CanUndo, (o) => { MoveBack(); });
            RedoCommand = new RelayCommand((o) => _historyProvider.CanRedo, (o) => { MoveForward(); });
            _globalFileManager = globalFileManager;
            _previousCurrentPath = globalFileManager.CurrentPath.Clone();

            globalFileManager.CurrentPathChanged += GlobalFileManager_CurrentPathChanged;
            Revalidate();
        }

        private void GlobalFileManager_CurrentPathChanged(object sender, bool triggeredByHistory)
        {
            if (!triggeredByHistory)
            {
                _historyProvider.Push(_previousCurrentPath);
            }

            _previousCurrentPath = _globalFileManager.CurrentPath.Clone();

            _historyProvider.CurrentItem = _previousCurrentPath;

            Revalidate();
        }

        private void Revalidate()
        {
            if (_historyProvider.CanRedo)
            {
                ForwardDirectoryPath = _historyProvider.RedoItem;
            }
            else
            {
                ForwardDirectoryPath = null;
            }

            if (_historyProvider.CanUndo)
            {
                BackDirectoryPath = _historyProvider.UndoItem;
            }
            else
            {
                BackDirectoryPath = null;
            }
        }

        public ICommand RedoCommand { get; }

        public ICommand UndoCommand { get; }

        public FormattedPath BackDirectoryPath
        {
            get
            {
                return _backDirectoryPath;
            }
            set
            {
                _backDirectoryPath = value;
                OnPropertyChanged();
            }
        }

        public FormattedPath ForwardDirectoryPath
        {
            get
            {
                return _forwardDirectoryPath;
            }
            set
            {
                _forwardDirectoryPath = value;
                OnPropertyChanged();
            }
        }

        private void MoveBack()
        {
            if (_historyProvider.CanUndo)
            {
                _globalFileManager.SetCurrentPath(_historyProvider.Undo(), true);
            }
        }

        private void MoveForward()
        {
            if (_historyProvider.CanRedo)
            {
                _globalFileManager.SetCurrentPath(_historyProvider.Redo(), true);
            }
        }
    }
}
