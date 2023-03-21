using System;
using System.IO;
using System.Linq;

namespace VisFileManager.Validators
{
    public static class FileNameValidator
    {
        public static bool ValidateName(string name, FormattedPath originalPath)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var validFileName = !name.Any(f => Path.GetInvalidFileNameChars().Contains(f));
            if (!validFileName)
            {
                return false;
            }

            var requestedName = Path.Combine(Path.GetDirectoryName(originalPath.Path), name);
            if (!SameOrWithDifferentCasing(requestedName, originalPath.Path))
            {
                return !ItemExists(originalPath.PathType).Invoke(requestedName);
            }

            return true;
        }

        private static bool SameOrWithDifferentCasing(string first, string second)
        {
            return (first == second)  || (first != second && first.ToUpper() == second.ToUpper());
        }

        private static Func<string, bool> ItemExists(PathValidator.PathType pathType)
        {
            if (pathType == PathValidator.PathType.Directory)
            {
                return Directory.Exists;
            }
            return File.Exists;
        }
    }
}
