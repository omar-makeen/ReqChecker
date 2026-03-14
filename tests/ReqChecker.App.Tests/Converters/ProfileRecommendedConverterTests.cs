using ReqChecker.App.Converters;
using System.Windows;

namespace ReqChecker.App.Tests.Converters;

public class ProfileRecommendedConverterTests
{
    private readonly ProfileRecommendedConverter _converter = new();

    [Fact]
    public void Convert_ShouldReturnVisible_ForDefaultProfileId()
    {
        // Arrange
        var defaultProfileId = "00000001-0000-0000-0000-000000000001";

        // Act
        var result = _converter.Convert(defaultProfileId, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_ShouldReturnCollapsed_ForOtherProfileIds()
    {
        // Arrange
        var otherProfileId = "00000002-0000-0000-0000-000000000002";

        // Act
        var result = _converter.Convert(otherProfileId, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_ShouldReturnCollapsed_ForNullValue()
    {
        // Act
        var result = _converter.Convert(null!, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_ShouldReturnCollapsed_ForNonStringValue()
    {
        // Act
        var result = _converter.Convert(123, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }
}
