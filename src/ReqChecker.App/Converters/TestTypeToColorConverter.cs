using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts test Type string to category color brush.
/// </summary>
public class TestTypeToColorConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string testType)
        {
            return testType switch
            {
                "Ping" or "HttpGet" or "DnsLookup" =>
                    Application.Current.FindResource("StatusInfo") as SolidColorBrush ?? new SolidColorBrush(Colors.Gray),
                "FileExists" or "DirectoryExists" =>
                    Application.Current.FindResource("StatusSkip") as SolidColorBrush ?? new SolidColorBrush(Colors.Gray),
                "ProcessList" or "RegistryRead" =>
                    Application.Current.FindResource("AccentSecondary") as SolidColorBrush ?? new SolidColorBrush(Colors.Gray),
                _ => Application.Current.FindResource("TextTertiary") as SolidColorBrush ?? new SolidColorBrush(Colors.Gray)
            };
        }
        return Application.Current.FindResource("TextTertiary") as SolidColorBrush ?? new SolidColorBrush(Colors.Gray);
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
