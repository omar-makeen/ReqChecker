using System.Globalization;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Chains two value converters together, applying them in sequence.
/// </summary>
public class ChainedConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the first converter in the chain.
    /// </summary>
    public IValueConverter FirstConverter { get; set; } = null!;

    /// <summary>
    /// Gets or sets the second converter in the chain.
    /// </summary>
    public IValueConverter SecondConverter { get; set; } = null!;

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = value;
        if (FirstConverter != null)
        {
            result = FirstConverter.Convert(result, targetType, parameter, culture);
        }
        if (SecondConverter != null)
        {
            result = SecondConverter.Convert(result, targetType, parameter, culture);
        }
        return result;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ChainedConverter does not support ConvertBack.");
    }
}
