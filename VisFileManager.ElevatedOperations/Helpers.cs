using System.IO;
using VisFileManager.Shared;

namespace VisFileManager.ElevatedOperations
{
    public static class Helpers
    {
        public static bool AnyFileLocked(string path)
        {
            var isDir = path.IsDirFile();
            if (isDir == true)
            {
                foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    if (IsFileLocked(file))
                    {
                        return true;
                    }
                }
            }
            else if (isDir == false)
            {
                return IsFileLocked(new FileInfo(path));
            }
            return false;

        }

        public static bool IsFileNameTaken(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
