using System.IO;
using VisFileManager.Validators;

namespace VisFileManager.Extensions
{
    public static class FormattedPathExtensions
    {
        public static FormattedPath ToFormattedPath(this DirectoryInfo info)
        {
            if (info == null)
                return null;
            return FormattedPath.CreateFormattedPath(info.FullName);
        }
    }

}
