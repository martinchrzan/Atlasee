using System;
using System.IO;

namespace VisFileManager.Shared
{
    public static class UniqueNameGeneratorHelper
    {
        private static string numberPattern = " ({0})";

        public static string NextAvailableName(string path)
        {
            // Short-cut if already available
            if (!FileOrDirectoryExists(path))
                return path;
            
            // If path has extension then insert the number pattern just before the extension and return next filename/directory
            if (Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern), FileOrDirectoryExists);

            // Otherwise just append the pattern to the path and return next filename/directory
            return GetNextFilename(path + numberPattern, FileOrDirectoryExists);
        }

        private static string GetNextFilename(string pattern, Func<string, bool> existenceCheck)
        {
            string tmp = string.Format(pattern, 1);
            if (tmp == pattern)
                throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!existenceCheck(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (existenceCheck(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }

        internal static bool FileOrDirectoryExists(string name)
        {
            return (Directory.Exists(name) || File.Exists(name));
        }
    }
}
