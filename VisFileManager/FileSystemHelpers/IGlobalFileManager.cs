using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VisFileManager.Validators;

namespace VisFileManager.FileSystemHelpers
{
    public interface IGlobalFileManager
    {
        // bool indicating if it was triggered by history control (undo/redo)
        event EventHandler<bool> CurrentPathChanged;

        void SetCurrentPath(FormattedPath currentPath, bool triggeredByHistory = false);

        FormattedPath CurrentPath { get; }

        event EventHandler SelectedItemsChanged;

        void SetSelectedItems(List<FormattedPath> selectedItems);

        List<FormattedPath> SelectedItems { get; }
        
        IEnumerable<FormattedPath> GetAllDirectoriesToParent(FormattedPath formattedPath);

        IEnumerable<DriveInfo> GetAllDrives();

        Task<(IEnumerable<DirectoryInfo> directories, IEnumerable<FileInfo> files)> GetAllFileEntries(FormattedPath rootPath, string filter = null, bool recursive = false);

        int GetNumberOfItemsInPath(FormattedPath path);
    }

}
