using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Converters;

public class ProfileRecommendedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string profileId)
        {
            return profileId == ProfileSelectorViewModel.DefaultProfileId ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
