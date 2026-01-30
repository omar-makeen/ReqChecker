using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ReqChecker.Core.Enums;

namespace ReqChecker.App.Converters;

/// <summary>
/// Converts FieldPolicyType to Visibility for UI elements.
/// - Editable: Visible (user can edit)
/// - Locked: Visible (read-only display)
/// - Hidden: Collapsed (not shown)
/// - PromptAtRun: Visible (will prompt during execution)
/// </summary>
public class FieldPolicyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FieldPolicyType policy)
        {
            // Hidden fields are always collapsed
            if (policy == FieldPolicyType.Hidden)
                return Visibility.Collapsed;

            // All other policies (Editable, Locked, PromptAtRun) are visible
            return Visibility.Visible;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
