using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VisFileManager.Validators;

namespace VisFileManager.FileSystemHelpers
{
    public interface IClipboardManager
    {
        void Copy(FormattedPath path);

        void Copy(IEnumerable<FormattedPath> paths);

        void Paste();

        void Cut(IEnumerable<FormattedPath> paths);

        void Cut(FormattedPath path);

        bool CanPaste { get; }

        ObservableCollection<string> PasteItems { get; }

        bool IsCut { get; }

        event EventHandler CanPasteChanged;
    }
}
