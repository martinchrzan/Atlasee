using System;
using System.Windows.Input;

namespace VisFileManager.ViewModelContracts
{
    public interface ISearchViewModel
    {
        string SearchTerm { get; }

        bool IsRecursive { get; }

        ICommand SearchCommand { get; }

        event EventHandler SearchRequested;
    }
}
