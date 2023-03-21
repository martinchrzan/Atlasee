using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace VisFileManager.Validators
{
    public class PathValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = Convert.ToString(value);

            var pathType = PathValidator.ValidatePath(strValue, out string formattedPath);

            if(pathType == PathValidator.PathType.Invalid)
            {
                return new ValidationResult(false, "Invalid path!");
            }
            return new ValidationResult(true, null);
        }
    }
}
