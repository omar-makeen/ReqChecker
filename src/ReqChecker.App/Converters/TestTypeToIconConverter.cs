using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts test Type string to SymbolRegular icon.
/// </summary>
public class TestTypeToIconConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string testType)
        {
            return testType switch
            {
                "Ping" => SymbolRegular.Wifi124,
                "HttpGet" => SymbolRegular.Globe24,
                "DnsLookup" => SymbolRegular.Link24,
                "DnsResolve" => SymbolRegular.Link24,
                "TcpPortOpen" => SymbolRegular.PlugConnected24,
                "UdpPortOpen" => SymbolRegular.Connected24,
                "MtlsConnect" => SymbolRegular.ShieldKeyhole24,
                "CertificateExpiry" => SymbolRegular.Certificate24,
                "WindowsService" => SymbolRegular.WindowApps24,
                "DiskSpace" => SymbolRegular.HardDrive20,
                "FileExists" => SymbolRegular.Document24,
                "DirectoryExists" => SymbolRegular.FolderOpen24,
                "ProcessList" => SymbolRegular.TaskListLtr24,
                "RegistryRead" => SymbolRegular.Settings24,
                _ => SymbolRegular.Beaker24  // Default fallback
            };
        }
        return SymbolRegular.Beaker24;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
