using System.Windows.Media;

namespace VisFileManager.Helpers
{
    public enum BackgroundType { SolidColor, Image}
    public class Background
    {
        public Background(string name, string id, Brush brush)
        {
            Name = name;
            Id = id;
            Brush = brush;
            BackgroundType = BackgroundType.Image;
        }

        public Background(string name, string id, string color)
        {
            Name = name;
            Id = id;
            Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            BackgroundType = BackgroundType.SolidColor;
        }

        public string Id { get; }

        public string Name { get; }

        public BackgroundType BackgroundType { get; }

        public Brush Brush { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
