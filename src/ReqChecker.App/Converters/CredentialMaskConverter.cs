using ReqChecker.Infrastructure.Export;
using System.Globalization;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts text to credential-masked text using the CredentialMasker.
/// </summary>
public class CredentialMaskConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string text)
        {
            return CredentialMasker.MaskCredentialsInText(text) ?? string.Empty;
        }

        return value;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("CredentialMaskConverter is one-way only.");
    }
}
