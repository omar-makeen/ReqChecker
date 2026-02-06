using System.Globalization;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts TimeSpan to formatted duration string.
/// Shows "Xms" for less than 1 second, "X.Xs" for 1 second or more.
/// </summary>
public class DurationFormatConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan duration)
        {
            // Handle 0-second edge case
            if (duration.TotalSeconds == 0)
            {
                return "0s";
            }

            // Show "Xms" for < 1s
            if (duration.TotalSeconds < 1)
            {
                return $"{duration.TotalMilliseconds:F0}ms";
            }
            // Show "Xm Ys" for >= 60s
            else if (duration.TotalSeconds >= 60)
            {
                int minutes = (int)duration.TotalMinutes;
                int seconds = duration.Seconds;
                return $"{minutes}m {seconds}s";
            }
            // Show "X.Xs" for 1-59s
            else
            {
                return $"{duration.TotalSeconds:F1}s";
            }
        }
        return string.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("DurationFormatConverter is one-way only.");
    }
}
