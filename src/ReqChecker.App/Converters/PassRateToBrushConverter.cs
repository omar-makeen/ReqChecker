using System.Globalization;
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

            // Determine opacity based on mode
            if (mode == "Background")
            {
                alpha = 0x33; // 20% opacity
            }
            else // Default to Foreground
            {
                alpha = 0xFF; // Full opacity
            }

            // Determine color based on pass rate thresholds
            byte r, g, b;
            if (passRate >= 80)
            {
                // Green: #10b981
                r = 0x10; g = 0xb9; b = 0x81;
            }
            else if (passRate >= 50)
            {
                // Amber: #f59e0b
                r = 0xf5; g = 0x9e; b = 0x0b;
            }
            else
            {
                // Red: #ef4444
                r = 0xef; g = 0x44; b = 0x44;
            }

            var brush = new SolidColorBrush(Color.FromArgb(alpha, r, g, b));
            brush.Freeze();
            return brush;
        }
        var defaultBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99)); // Default gray
        defaultBrush.Freeze();
        return defaultBrush;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("PassRateToBrushConverter is one-way only.");
    }
}
