using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts boolean to opacity: true = 1.0, false = 0.5.
/// </summary>
public class BoolToOpacityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? 1.0 : 0.5;
        }

        return 1.0; // Default to full opacity
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not supported for BoolToOpacityConverter.");
    }
}
