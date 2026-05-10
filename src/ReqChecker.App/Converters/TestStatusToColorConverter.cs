using ReqChecker.Core.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts TestStatus enum to color brushes.
/// </summary>
public class TestStatusToColorConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TestStatus status)
        {
            var isGlow = parameter?.ToString() == "GlowColor";
            var color = status switch
            {
                TestStatus.Pass => isGlow
                    ? (Color)Application.Current.FindResource("StatusPassGlowColor")
                    : (Color)Application.Current.FindResource("StatusPassColor"),
                TestStatus.Fail => isGlow
                    ? (Color)Application.Current.FindResource("StatusFailGlowColor")
                    : (Color)Application.Current.FindResource("StatusFailColor"),
                TestStatus.Skipped => isGlow
                    ? (Color)Application.Current.FindResource("StatusSkipGlowColor")
                    : (Color)Application.Current.FindResource("StatusSkipColor"),
                _ => (Color)Application.Current.FindResource("StatusInfoColor")
            };

            if (targetType == typeof(Color) || targetType == typeof(Color?))
                return color;

            return new SolidColorBrush(color);
        }

        return new SolidColorBrush(Colors.Gray);
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
