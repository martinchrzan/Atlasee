using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using VisFileManager.Extensions;
using VisFileManager.Shared;
using static VisFileManager.Validators.PathValidator;

namespace VisFileManager.Validators
{
    // Implements INotifyPropertyChanged to avoid potential binding leaks
    public class FormattedPath : INotifyPropertyChanged
    {
        private const string MyComputerName = "My Computer";

        public event PropertyChangedEventHandler PropertyChanged;

        private FormattedPath(string path, PathType pathType)
        {
            Path = FixFilePathCasing(path);
            PathType = pathType;
            if(PathType == PathType.Directory)
            {
                Name = new DirectoryInfo(Path).Name;
            }
            else if(PathType == PathType.File)
            {
                Name = System.IO.Path.GetFileName(path);
                ExtensionWithDot = System.IO.Path.GetExtension(path);
            }
            else if(PathType == PathType.MyComputer)
            {
                Name = MyComputerName;
            }
        }

        private FormattedPath(string path, PathType pathType, string name)
        {
            Path = FixFilePathCasing(path);
            PathType = pathType;
            Name = name;
        }
        
        public static FormattedPath CreateFormattedPath(string path)
        {
            var pathType = ValidatePath(path, out string formatted);
            return new FormattedPath(formatted, pathType);
        }

        public static FormattedPath CreateFormattedPath(DriveInfo driveInfo)
        {
            return new FormattedPath(driveInfo.RootDirectory.FullName, PathType.Drive, driveInfo.GetDriveName());
        }

        public string Path { get; private set; }

        public PathType PathType { get; }

        public string Name { get; private set; }

        public string ExtensionWithDot { get; private set; }

        public FormattedPath Parent
        {
            get
            {
                if (PathType == PathType.Directory)
                {
                    return new DirectoryInfo(Path).Parent.ToFormattedPath();
                }
                else if (PathType == PathType.File)
                {
                    return new FileInfo(Path).Directory.ToFormattedPath();
                }

                return null;
            }
        }

        public FormattedPath Clone()
        {
            return new FormattedPath(Path, PathType, Name);
        }

        private string FixFilePathCasing(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            try
            {
                string fullFilePath = System.IO.Path.GetFullPath(filePath);

                string fixedPath = "";
                foreach (string token in fullFilePath.Split('\\'))
                {
                    //first token should be drive token
                    if (fixedPath == "")
                    {
                        //fix drive casing
                        string drive = string.Concat(token, "\\");
                        drive = DriveInfo.GetDrives()
                            .First(driveInfo => driveInfo.Name.Equals(drive, StringComparison.OrdinalIgnoreCase)).Name;

                        fixedPath = drive;
                    }
                    else
                    {
                        var fileSystemEntries = Directory.GetFileSystemEntries(fixedPath, token);
                        if (fileSystemEntries.Any())
                        {
                            fixedPath = fileSystemEntries.First();
                        }
                    }
                }

                return fixedPath;
            }
            catch(Exception ex)
            {
                Logger.LogError("Failed to fix path casing: " + filePath + Environment.NewLine + ex.Message);
                return filePath;
            }
        }
    }
}
