using ReqChecker.Core.Enums;
using System.Globalization;
using System.Windows.Data;

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
            return status switch
            {
                TestStatus.Pass => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)),   // #4CAF50
                TestStatus.Fail => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69)),     // #DC3545
                TestStatus.Skipped => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)), // #6B7280
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray)
            };
        }

        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
