using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.IO;
using VisFileManager.Shared;

namespace VisFileManager.Validators
{
    public static class PathValidator
    {
        public enum PathType { File, Directory, Invalid, MyComputer, Drive };

        public static PathType ValidatePath(string path, out string formattedPath)
        {
            if (string.IsNullOrEmpty(path))
            {
                formattedPath = string.Empty;
                return PathType.MyComputer;
            }

            //check if we entered environmental variable
            var expanded = Environment.ExpandEnvironmentVariables(path);
            var isDirExpanded = SharedFunctions.IsDirFile(expanded);

            if (expanded != path && isDirExpanded != null)
            {
                formattedPath = expanded;
                return (bool)isDirExpanded ? PathType.Directory : PathType.File;
            }

            //entered known name? 
            var knownNamePath = KnownNamesProvider.GetKnownNamePath(path);
            if (!string.IsNullOrEmpty(knownNamePath))
            {
                formattedPath = knownNamePath;
                return PathType.Directory;
            }

            // we only got drive letter
            if (path.EndsWith(":"))
            {
                path = path + "\\";
            }

            // we ignore path that is not absolute
            if (!IsPathFullyQualified(path))
            {
                formattedPath = string.Empty;
                return PathType.Invalid;
            }

            // does it exist? 
            var isDir = SharedFunctions.IsDirFile(path);
            if (isDir == null)
            {
                formattedPath = string.Empty;
                return PathType.Invalid;
            }

            formattedPath = Path.GetFullPath(path);

            // remove extra backslashes from end if path is not drive only for example - c:\
            if (Path.GetPathRoot(formattedPath) != formattedPath && formattedPath.EndsWith("\\"))
            {
                // backslashes at the end will cause that parent of this directory will be the same directory but without ending backslashes, therefore remove them
                formattedPath = formattedPath.TrimEnd(new char[] { '\\' });
            }

            return (bool)isDir ? PathType.Directory : PathType.File;
        }
        
        private static bool IsPathFullyQualified(string path)
        {
            try
            {
                var root = Path.GetPathRoot(path);
                return root.StartsWith(@"\\") || root.EndsWith(@"\");
            }
            catch(ArgumentException)
            {
                return false;
            }
        }
    }
}
