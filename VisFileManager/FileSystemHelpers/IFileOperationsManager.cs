using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VisFileManager.Validators;

namespace VisFileManager.FileSystemHelpers
{
    public interface IFileOperationsManager
    {
        Task<bool> RemoveItems(IEnumerable<FormattedPath> itemsToRemove, CancellationTokenSource cancellationTokenSource);

        Task<bool> Rename(FormattedPath oldName, string newName, CancellationTokenSource cancellationTokenSource, Action<bool> runOnResult = null);

        Task<bool> CreateNewFolder(string destinationPath, CancellationTokenSource cancellationTokenSource);

        Task<bool> CreateNewTextFile(string destinationPath, CancellationTokenSource cancellationTokenSource);

        Task<bool> Copy(IEnumerable<FormattedPath> itemsToCopy, FormattedPath destinationFolder, CancellationTokenSource cancellationTokenSource, Action<double, double, string, string> progressReport);

        Task<bool> Move(IEnumerable<FormattedPath> itemsToMove, FormattedPath destinationFolder, CancellationTokenSource cancellationTokenSource, Action<double, double, string, string> progressReport);
    }
}
