namespace VisFileManager.FileSystemHelpers
{
    public class ItemChangedInfo
    {
        public ItemChangedInfo(string newPath, string oldPath)
        {
            NewPath = newPath;
            OldPath = oldPath;
        }

        public ItemChangedInfo(string newPath)
        {
            NewPath = newPath;
        }

        public string NewPath { get; }
        public string OldPath { get; }
    }
}
