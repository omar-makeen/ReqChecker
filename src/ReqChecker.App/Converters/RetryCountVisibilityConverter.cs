using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts int attempt count to Visibility.
/// Shows retry count only when AttemptCount > 1.
/// </summary>
public class RetryCountVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int attemptCount)
        {
            // Show only when attempt count > 1 (meaning there were retries)
            return attemptCount > 1 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("RetryCountVisibilityConverter is one-way only.");
    }
}
