using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts pass rate (double 0-100) to color-coded SolidColorBrush.
/// Returns green (80%+), amber (50-79%), or red (below 50%) based on thresholds.
/// Supports "Background" parameter for 20% opacity variant.
/// </summary>
public class PassRateToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double passRate)
        {
            byte alpha;
            string? mode = parameter as string;

            if (mode == "Background")
            {
                alpha = 0x33;
            }
            else
            {
                alpha = 0xFF;
            }

            var baseColor = passRate switch
            {
                >= 80 => (Color)Application.Current.FindResource("StatusPassColor"),
                >= 50 => (Color)Application.Current.FindResource("StatusSkipColor"),
                _ => (Color)Application.Current.FindResource("StatusFailColor")
            };

            var color = Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
        var defaultBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));
        defaultBrush.Freeze();
        return defaultBrush;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("PassRateToBrushConverter is one-way only.");
    }
}
