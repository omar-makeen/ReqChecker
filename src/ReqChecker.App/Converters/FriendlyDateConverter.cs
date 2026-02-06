using System.Globalization;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts DateTimeOffset to friendly relative date string.
/// Shows "Today at h:mm tt" for same calendar day,
/// "Yesterday at h:mm tt" for previous calendar day,
/// or "MMM d, yyyy 'at' h:mm tt" for older dates.
/// </summary>
public class FriendlyDateConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            var localDate = dateTimeOffset.LocalDateTime.Date;
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            if (localDate == today)
            {
                return dateTimeOffset.LocalDateTime.ToString("Today at h:mm tt", CultureInfo.InvariantCulture);
            }
            else if (localDate == yesterday)
            {
                return dateTimeOffset.LocalDateTime.ToString("Yesterday at h:mm tt", CultureInfo.InvariantCulture);
            }
            else
            {
                return dateTimeOffset.LocalDateTime.ToString("MMM d, yyyy 'at' h:mm tt", CultureInfo.InvariantCulture);
            }
        }
        return string.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("FriendlyDateConverter is one-way only.");
    }
}
