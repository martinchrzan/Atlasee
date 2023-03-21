using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using VisFileManager.Common;
using VisFileManager.Helpers;

namespace VisFileManager.Extensions
{
    public class UpdatePropertySourceWhenEnterPressedExtension : MarkupExtension
    {
        public UpdatePropertySourceWhenEnterPressedExtension()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new RelayCommand((sender) =>
            {
                ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
                WpfUtilities.FocusMainWindow();
            });
        }
    }
}
