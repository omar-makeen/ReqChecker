using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts boolean values to Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to Visibility.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter. Use "Inverse" to invert the boolean before conversion.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>Visible if true, Collapsed if false. If parameter is "Inverse", the logic is reversed.</returns>
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        var isInverse = parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase);
        var boolValue = value is bool b && b;

        if (isInverse)
            boolValue = !boolValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Converts Visibility back to boolean.
    /// </summary>
    /// <param name="value">The Visibility value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>True if Visible, false otherwise.</returns>
    public object ConvertBack(object value, Type? targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility visibility && visibility == Visibility.Visible;
    }
}
