using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisFileManager.Shared;

namespace VisFileManager.FileSystemHelpers
{
    public static class SafeFilesEnumerator
    {
        public static IEnumerable<FileInfo> EnumerateFiles(DirectoryInfo directory, string searchPattern, SearchOption searchOpt)
        {
            try
            {
                var dirFiles = Enumerable.Empty<FileInfo>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = directory.EnumerateDirectories()
                                        .SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(directory.EnumerateFiles(searchPattern));
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogWarning("Enumerate files failed with " + ex.Message + ex.InnerException);
                return Enumerable.Empty<FileInfo>();
            }
        }

        public static IEnumerable<DirectoryInfo> EnumerateDirectories(DirectoryInfo directory, string searchPattern, SearchOption searchOpt)
        {
            try
            {
                var dirFiles = Enumerable.Empty<DirectoryInfo>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = directory.EnumerateDirectories()
                                        .SelectMany(x => EnumerateDirectories(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(directory.EnumerateDirectories(searchPattern));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Enumerable.Empty<DirectoryInfo>();
            }
        }
    }
}
