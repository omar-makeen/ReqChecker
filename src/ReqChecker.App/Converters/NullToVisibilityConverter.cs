using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts null/empty to Collapsed and non-null to Visible.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isNull = value == null || (value is string str && string.IsNullOrEmpty(str));
        var invert = parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase);

        if (invert)
        {
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }

        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
