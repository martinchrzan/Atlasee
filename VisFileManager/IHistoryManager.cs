using System.Windows.Input;
using VisFileManager.Validators;

namespace VisFileManager
{
    public interface IHistoryManager
    {
        ICommand UndoCommand { get; }

        ICommand RedoCommand { get; }

        FormattedPath BackDirectoryPath { get; }

        FormattedPath ForwardDirectoryPath { get; }
    }
}
