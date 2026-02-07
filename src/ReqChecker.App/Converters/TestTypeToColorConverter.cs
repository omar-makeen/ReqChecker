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
    private static readonly SolidColorBrush FallbackBrush = new(Colors.Gray);

    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string testType)
        {
            return testType switch
            {
                "Ping" or "HttpGet" or "DnsLookup" or "DnsResolve" or "TcpPortOpen" =>
                    Application.Current.FindResource("StatusInfo") as SolidColorBrush ?? FallbackBrush,
                "FileExists" or "DirectoryExists" or "DiskSpace" =>
                    Application.Current.FindResource("StatusSkip") as SolidColorBrush ?? FallbackBrush,
                "ProcessList" or "RegistryRead" or "WindowsService" =>
                    Application.Current.FindResource("AccentSecondary") as SolidColorBrush ?? FallbackBrush,
                _ => Application.Current.FindResource("TextTertiary") as SolidColorBrush ?? FallbackBrush
            };
        }
        return Application.Current.FindResource("TextTertiary") as SolidColorBrush ?? FallbackBrush;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
