using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    [Export(typeof(ISearchViewModel))]
    public class SearchViewModel : ViewModelBase, ISearchViewModel
    {
        private string _searchTerm;
        private bool _isRecursive;

        [ImportingConstructor]
        public SearchViewModel()
        {
            SearchCommand = new RelayCommand(() => { OnSearchRequestedRaised(); });
        }

        public string SearchTerm
        {
            get
            {
                return _searchTerm;
            }
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }

        public bool IsRecursive
        {
            get
            {
                return _isRecursive;
            }
            set
            {
                _isRecursive = value;
                OnSearchRequestedRaised();
                OnPropertyChanged();
            }
        }

        public event EventHandler SearchRequested;

        private void OnSearchRequestedRaised()
        {
            SearchRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
