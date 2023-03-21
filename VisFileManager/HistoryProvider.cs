using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VisFileManager
{
    public class HistoryProvider<T> : INotifyPropertyChanged
    {
        private Stack<T> _undoHistory = new Stack<T>();
        private Stack<T> _redoHistory = new Stack<T>();
        
        public event EventHandler HistoryChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public T CurrentItem { get; set; }

        public T UndoItem
        {
            get
            {
                if(_undoHistory.Count>0)
                {
                    return _undoHistory.Peek();
                }
                return default(T);
            }
        }

        public T RedoItem
        {
            get
            {
                if (_redoHistory.Count > 0)
                {
                    return _redoHistory.Peek();
                }
                return default(T);
            }
        }

        public bool CanUndo
        {
            get { return _undoHistory.Count > 0; }
        }

        public bool CanRedo
        {
            get { return _redoHistory.Count > 0; }
        }

        public void Push(T item)
        {
            _redoHistory.Clear();
            _undoHistory.Push(item);
            OnHistoryChanged();
        }

        public T Undo()
        {
            if (_undoHistory.Count > 0)
            {
                var undoItem = _undoHistory.Pop();
                _redoHistory.Push(CurrentItem);
                CurrentItem = undoItem;
                OnHistoryChanged();
                return undoItem;
            }
            return default(T);
        }

        public T Redo()
        {
            if (_redoHistory.Count > 0)
            {
                var redoItem = _redoHistory.Pop();
                _undoHistory.Push(CurrentItem);
                CurrentItem = redoItem;
                OnHistoryChanged();
                return redoItem;
            }
            return default(T);
        }

        private void OnHistoryChanged()
        {
            HistoryChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged("CanUndo");
            RaisePropertyChanged("CanRedo");
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
