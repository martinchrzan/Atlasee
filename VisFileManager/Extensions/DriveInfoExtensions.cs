using System;
using System.IO;
using System.Text;
using VisFileManager.Helpers;
using VisFileManager.Shared;

namespace VisFileManager.Extensions
{
    public static class DriveInfoExtensions
    {
        public static string GetDriveName(this DriveInfo driveInfo)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(driveInfo.VolumeLabel))
                {
                    sb.Append(driveInfo.VolumeLabel);
                }
                else
                {
                    sb.Append(driveInfo.DriveType);
                }
            }
            catch(Exception ex)
            {
                Logger.LogWarning("Drive no name" + Environment.NewLine + ex.Message);
                sb.Append("NO NAME");
            }

            sb.Append(string.Format(" ({0})", driveInfo.Name.Replace("\\", string.Empty)));
            return sb.ToString();
        }

        public static string GetDriveType(this DriveInfo driveInfo)
        {
            if (driveInfo.DriveType == DriveType.Fixed)
            {
                return "Internal";
            }

            else return driveInfo.DriveType.ToString();
        }
    }

}
