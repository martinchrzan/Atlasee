using System;
using System.Windows.Markup;

namespace VisFileManager.Extensions
{
    public class OpacityExtension : MarkupExtension
    {
        public System.Windows.Media.Color Color { get; set; }
        public byte Opacity { get; set; } // defaults to 0, so you don't have 
                                          // to set it for the color to be transparent

        public OpacityExtension()
        {
        }
        
        public OpacityExtension(System.Windows.Media.Color color)
        {
            Color = color;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return System.Windows.Media.Color.FromArgb(Opacity, this.Color.R, this.Color.G, this.Color.B);
        }
    }
}
