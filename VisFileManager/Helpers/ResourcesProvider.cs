using System.Windows.Media;

namespace VisFileManager.Helpers
{
    public static class ResourcesProvider
    {
        private static ImageSource _folderIcon;
        public static ImageSource GetFolderIcon()
        {
            if(_folderIcon == null)
            {
                _folderIcon = (ImageSource)App.Current.FindResource("FolderIcon");
                _folderIcon.Freeze();
            }
            return _folderIcon;
        }
    }
}
