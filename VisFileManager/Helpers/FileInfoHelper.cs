using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.InteropServices;
using VisFileManager.Shared;
using VisFileManager.Validators;

namespace VisFileManager.Helpers
{
    [Export(typeof(FileInfoHelper))]
    public class FileInfoHelper
    {
        public enum PerceivedTypeEnum
        {
            Custom = -3,
            Unspecified = -2,
            Folder = -1,
            Unknown = 0,
            Text = 1,
            Image = 2,
            Audio = 3,
            Video = 4,
            Compressed = 5,
            Document = 6,
            System = 7,
            Application = 8,
            GameMedia = 9,
            Contacts = 10,

            Shortcut = 100
        }

        public static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        /// <summary>
        /// Returns in 100ns units not milliseconds
        /// </summary>
        /// <param name="formattedPath"></param>
        /// <returns></returns>
        public ulong? GetMediaDuration(FormattedPath formattedPath)
        {
            try
            {
                using(var file = GetShellFile(formattedPath))
                {
                    return file.Properties.System.Media.Duration.Value;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetMediaDurationFormatted(FormattedPath formattedPath)
        {
            var file = GetMediaDuration(formattedPath);
            if(file!= null)
            {
                return TimeSpan.FromMilliseconds(file.Value / 10000).ToString(@"hh\:mm\:ss");
            }
            return string.Empty;
        }

        public ulong? GetFileSize(FormattedPath formattedPath)
        {
            using (var file = GetShellFile(formattedPath))
            {
                return file.Properties.System?.Size?.Value.Value;
            }
        }

        public string GetFileSizeFormatted(FormattedPath formattedPath)
        {
            var file = GetFileSize(formattedPath);

            if (file != null)
            {
                return ToSizeWithSuffix(file.Value);
            }
            return string.Empty;
        }

        public ulong? GetFileSizeOnDisk(FormattedPath formattedPath)
        {
            using (var file = GetShellFile(formattedPath))
            {
                return file.Properties.System.FileAllocationSize.Value;
            }
        }

        public string GetFileSizeOnDiskFormatted(FormattedPath formattedPath)
        {
            var file = GetFileSizeOnDisk(formattedPath);
            if(file != null)
            {
                return ToSizeWithSuffix(file.Value);
            }
            return string.Empty;
        }
        
        public string GetFileSizeInKBFormatted(FormattedPath formattedPath)
        {
            ulong reportedSize = 0;
            if (formattedPath.PathType == PathValidator.PathType.File)
            {
                var file = GetFileSize(formattedPath);
                if (file != null)
                {
                    reportedSize = file.Value;
                }
            }
            else if (formattedPath.PathType == PathValidator.PathType.Directory)
            {
                reportedSize = GetDirectoryInfo(formattedPath).Size;
            }
            else
            {
                return string.Empty;
            }

            var value = 1;
            if (reportedSize == 0)
            {
                value = 0;
            }
            else if (reportedSize > 1024)
            {
                value = (int)Math.Ceiling((reportedSize / 1024.0));
            }
            return string.Format("{0} KB", value);
        }

        public DirectorySizeInfo GetDirectoryInfo(FormattedPath formattedPath)
        {
            return DirSize(new DirectoryInfo(formattedPath.Path));  
        }

        public class DirectorySizeInfo
        {
            public ulong FileCount = 0;
            public ulong DirectoryCount = 0;
            public ulong Size = 0;

            public static DirectorySizeInfo operator +(DirectorySizeInfo s1, DirectorySizeInfo s2)
            {
                return new DirectorySizeInfo()
                {
                    DirectoryCount = s1.DirectoryCount + s2.DirectoryCount,
                    FileCount = s1.FileCount + s2.FileCount,
                    Size = s1.Size + s2.Size
                };
            }
        }

        private static DirectorySizeInfo DirSize(DirectoryInfo d)
        {
            DirectorySizeInfo size = new DirectorySizeInfo();

            try
            {
                // Add file sizes.
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size.Size += (ulong)fi.Length;
                }
                size.FileCount += (ulong)fis.Length;

                // Add subdirectory sizes.
                DirectoryInfo[] dis = d.GetDirectories();
                size.DirectoryCount += (ulong)dis.Length;
                foreach (DirectoryInfo di in dis)
                {
                    size += DirSize(di);
                }
            }
            catch(Exception ex)
            {
                Logger.LogError("Failed to get directory size "+ d.FullName + Environment.NewLine + ex.Message);
            }

            return size;
        }


        public static string ToSizeWithSuffix(ulong value, int decimalPlaces = 1)
        {
            var returnValue = ToSizeWithSuffixWithoutOriginalValue(value, decimalPlaces);
            return string.Format("{0} ({1:N0} bytes)", returnValue, value);
        }

        public static string ToSizeWithSuffixWithoutOriginalValue(ulong value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException(nameof(decimalPlaces)); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                                adjustedSize,
                                SizeSuffixes[mag]);
        }

        public PerceivedTypeEnum GetGeneralFileType(FormattedPath formattedPath)
        {
            if (formattedPath.PathType == PathValidator.PathType.Directory)
            {
                return PerceivedTypeEnum.Folder;
            }

            using (var file = GetShellObject(formattedPath))
            {
                if (file.IsLink)
                {
                    return PerceivedTypeEnum.Shortcut;
                }
                return (PerceivedTypeEnum)file.Properties.System.PerceivedType.Value;
            }
        }

        public string GetSpecificFileType(FormattedPath formattedPath)
        {
            try
            {
                var generalType = GetGeneralFileType(formattedPath);
                if (generalType == PerceivedTypeEnum.Unspecified || generalType == PerceivedTypeEnum.Unknown)
                {
                    using (var file = GetShellObject(formattedPath))
                    {
                        return file.Properties.System.ItemTypeText.Value;
                    }
                }

                return generalType.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to get specific type of a file", ex);
            }
            return string.Empty;
        }


        public DateTime? GetCreateDate(FormattedPath formattedPath)
        {
            using (var item = GetShellObject(formattedPath))
            {
                return item.Properties.System.DateCreated.Value;
            }
        }

        public DateTime? GetAccessedDate(FormattedPath formattedPath)
        {
            using (var item = GetShellFile(formattedPath))
            {
                return item.Properties.System.DateAccessed.Value;
            }
        }

        public DateTime? GetModifiedDate(FormattedPath formattedPath)
        {
            if(formattedPath.PathType == PathValidator.PathType.Directory)
            {
                //TODO: check inner items for the latest modified date! 
                return null;
            }

            using (var item = GetShellFile(formattedPath))
            {
                return item.Properties.System.DateModified.Value;
            }
        }

        public int GetImageWidth(FormattedPath formattedPath)
        {
            try
            {
                using (var item = GetShellFile(formattedPath))
                {
                    if (item.Properties.System.Image.HorizontalSize.Value.HasValue)
                    {
                        return (int)item.Properties.System.Image.HorizontalSize.Value.Value;
                    }
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int GetImageHeight(FormattedPath formattedPath)
        {
            try
            {
                using (var item = GetShellFile(formattedPath))
                {
                    if (item.Properties.System.Image.VerticalSize.Value.HasValue)
                    {
                        return (int)item.Properties.System.Image.VerticalSize.Value.Value;
                    }
                }
                return -1;
            }
            catch(Exception)
            {
                return -1; 
            }
}

        public uint? GetImageBitDepth(FormattedPath formattedPath)
        {
            try
            {
                using (var item = GetShellFile(formattedPath))
                {
                    return item.Properties.System.Image.BitDepth.Value;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public uint? GetVideoWidth(FormattedPath formattedPath)
        {
            try
            {
                using (var item = GetShellFile(formattedPath))
                {
                    return item.Properties.System.Video.FrameWidth.Value;
                }
            }
            catch(Exception)
            {
                return null; 
            }
        }

        public uint? GetVideoHeight(FormattedPath formattedPath)
        {
            try
            {
                using (var item = GetShellFile(formattedPath))
                {
                    return item.Properties.System.Video.FrameHeight.Value;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public uint? GetVideoFrameratePerMillisecond(FormattedPath formattedPath)
        {
            try
            {
                using (var item = GetShellFile(formattedPath))
                {
                    return item.Properties.System.Video.FrameRate.Value;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetVideoFrameratePerSecondFormatted(FormattedPath formattedPath)
        {
            var returnValue = GetVideoFrameratePerMillisecond(formattedPath);
            if(returnValue != null)
            {
                return string.Format("{0:N2} frames/second", returnValue / 1000);
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns video bitrate in bits per second
        /// </summary>
        /// <param name="formattedPath"></param>
        /// <returns></returns>
        public uint? GetVideoBitratePerSecond(FormattedPath formattedPath)
        {
            using (var item = GetShellFile(formattedPath))
            {
                return item.Properties.System.Video.TotalBitrate.Value;
            }
        }

        public string GetVideoBitratePerSecondFormatted(FormattedPath formattedPath)
        {
            var resultValue = GetVideoBitratePerSecond(formattedPath);
            if(resultValue != null)
            {
                return string.Format("{0} kbps", resultValue/ 1000);
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns audio bitrate in bits per second
        /// </summary>
        /// <param name="formattedPath"></param>
        /// <returns></returns>
        public uint? GetAudioBitratePerSecond(FormattedPath formattedPath)
        {
            using (var item = GetShellFile(formattedPath))
            {
                return item.Properties.System.Audio.EncodingBitrate.Value;
            }
        }

        public string GetAudioBitratePerSecondFormatted(FormattedPath formattedPath)
        {
            var resultValue = GetAudioBitratePerSecond(formattedPath);
            if (resultValue != null)
            {
                return string.Format("{0} kbps", resultValue / 1000);
            }
            return string.Empty;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        public bool ShowFileProperties(string filename)
        {
            try
            {
                SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
                info.cbSize = Marshal.SizeOf(info);
                info.lpVerb = "properties";
                info.lpFile = filename;
                info.nShow = SW_SHOW;
                info.fMask = SEE_MASK_INVOKEIDLIST;
                return ShellExecuteEx(ref info);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to open file properties " + filename + Environment.NewLine + ex.Message);
                return false;
            }
        }

        private ShellFile GetShellFile(FormattedPath formattedPath)
        {
            return ShellFile.FromFilePath(formattedPath.Path);
        }

        private ShellObject GetShellObject(FormattedPath formattedPath)
        {
            return ShellObject.FromParsingName(formattedPath.Path);
        }
    }
}
