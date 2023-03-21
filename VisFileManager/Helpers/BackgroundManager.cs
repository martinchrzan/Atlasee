using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VisFileManager.Settings;

namespace VisFileManager.Helpers
{
    [Export(typeof(IBackgroundManager))]
    public class BackgroundManager : IBackgroundManager
    {
        private readonly IUserSettings _settings;
        public const string defaultBackgroundId = "default";

        [ImportingConstructor]
        public BackgroundManager(IUserSettings settings)
        {
            PopulateBackground();
            var selectedBackgroundId = settings.BackgroundId?.Value;

            SelectedBackground = string.IsNullOrEmpty(selectedBackgroundId)
                ? Backgrounds.First(it => it.Id == defaultBackgroundId)
                : Backgrounds.FirstOrDefault(it => it.Id == selectedBackgroundId) ?? Backgrounds.First(it => it.Id == defaultBackgroundId);
            _settings = settings;
        }
        public event EventHandler OnSelectedBackgroundChanged;

        public ObservableCollection<Background> Backgrounds { get; } = new ObservableCollection<Background>();

        public Background SelectedBackground { get; private set; }

        public void SetBackground(Background background)
        {
            SelectedBackground = background;
            _settings.BackgroundId.Value = background.Id;
            OnSelectedBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PopulateBackground()
        {
            var defaultBrush = new ImageBrush() 
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Backgrounds/defaultBackground.jpg")),
                Stretch = Stretch.UniformToFill
            };

            var greenBrush = new ImageBrush()
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Backgrounds/greenBackground.png")),
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 297, 297),
                ViewportUnits = BrushMappingMode.Absolute,
            };

           
            var grayBrush = new ImageBrush()
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Backgrounds/grayBackground.png")),
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 397, 322),
                ViewportUnits = BrushMappingMode.Absolute,
            };

            var radialBrush = new RadialGradientBrush()
            {
                Center = new Point(0.6, 0.5),
                GradientStops = new GradientStopCollection()
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString("#886876"), 0.2),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#4f6177"), 0.8)
                }
            };

            var radialGreenBrush = new RadialGradientBrush()
            {
                Center = new Point(0.6, 0.5),
                GradientStops = new GradientStopCollection()
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString("#248a65"), 0.5),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#1a5741"), 1)
                }
            };

            Backgrounds.Add(new Background("Default", defaultBackgroundId, defaultBrush));
            Backgrounds.Add(new Background("Green squares", "image_green", greenBrush));
            Backgrounds.Add(new Background("Green radial", "image_radial_green", radialGreenBrush));
            Backgrounds.Add(new Background("Gray squares", "image_gray", grayBrush));
            Backgrounds.Add(new Background("Sunset", "image_radial", radialBrush));
            Backgrounds.Add(new Background("Dark", "color_202020", "#202020"));
            
        }

    }
}
