using System.Globalization;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts null/empty to false and non-null to true.
/// Use parameter "Invert" to reverse the logic.
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isNull = value == null || (value is string str && string.IsNullOrEmpty(str));
        var invert = parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase);

        if (invert)
        {
            return isNull;
        }

        return !isNull;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
