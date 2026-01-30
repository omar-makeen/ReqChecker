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
        if (value == null)
        {
            return Visibility.Collapsed;
        }

        if (value is string str && string.IsNullOrEmpty(str))
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
