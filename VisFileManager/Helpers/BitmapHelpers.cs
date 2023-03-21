using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VisFileManager.Validators;
using static VisFileManager.Helpers.InvokeHelper;

namespace VisFileManager.Helpers
{
    public static class BitmapHelpers
    {
        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Pbgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        private static IEnumerable<string> recognisedImageExtensions = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders().SelectMany(codec => codec.FilenameExtension.ToUpperInvariant().Split(';'));

        public static bool IsRecognisedImageFile(string fileName)
        {
            string targetExtension = System.IO.Path.GetExtension(fileName);
            if (String.IsNullOrEmpty(targetExtension))
            {
                return false;
            }

            targetExtension = "*" + targetExtension.ToUpperInvariant();
            return recognisedImageExtensions.Contains(targetExtension);
        }

        public static BitmapSource GetItemIcon(string fullPath, bool freeze, bool bigSize)
        {
            var thumbnail = IsRecognisedImageFile(fullPath) ? ThumbnailOptions.None : ThumbnailOptions.IconOnly;
            var bitmap = WindowsThumbnailProvider.GetThumbnail(fullPath, 85, 69, thumbnail);
            var icon = loadBitmap(bitmap);
            bitmap.Dispose();
            icon.Freeze();
            return icon;
            //BitmapSource icon;

            //using (var shellObject = ShellObject.FromParsingName(fullPath))
            //{
            //    if (bigSize)
            //    {
            //        var ic = shellObject.Thumbnail;
            //        ic.AllowBiggerSize = false;
            //        icon = (ic.MediumBitmapSource);
            //    }
            //    else
            //    {
            //        icon = shellObject.Thumbnail.SmallBitmapSource;
            //    }
            //}

            //if (freeze)
            //{
            //    icon.Freeze();
            //}
            //return icon;
        }

        public static BitmapSource OpenAsInfoIconToBitmapSource(OpenWithInfo openAsInfo)
        {
            if (string.IsNullOrEmpty(openAsInfo.IconPath))
            {
                return null;
            }

            // try getting icon from standard app 
            try
            {
                IntPtr iconLarge = new IntPtr();
                IntPtr iconSmall = new IntPtr();
                ExtractIconEx(openAsInfo.IconPath, openAsInfo.IconIndex, ref iconLarge, ref iconSmall, 1);
                return BitmapToBitmapSource(System.Drawing.Icon.FromHandle(iconLarge).ToBitmap());
            }
            catch
            {
                // it was probably resource
            }
            // try getting icon from embedded resource file (UWP apps)
            try
            {
                StringBuilder sb = new StringBuilder(1000);
                SHLoadIndirectString(openAsInfo.IconPath, sb, sb.Capacity, IntPtr.Zero);

                return new BitmapImage(new Uri(sb.ToString()));
            }
            catch
            {

            }
            return null;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        internal static extern int ExtractIconEx(string stExeFileName, int nIconIndex, ref IntPtr phiconLarge, ref IntPtr phiconSmall, int nIcons);

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        internal static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

    }
}
