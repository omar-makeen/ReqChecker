using System.Globalization;
using ReqChecker.App.Converters;
using System.Windows;

namespace ReqChecker.App.Tests.Converters;

/// <summary>
/// Unit tests for CountToVisibilityConverter including Invert parameter support.
/// </summary>
public class CountToVisibilityConverterTests
{
    private readonly CountToVisibilityConverter _converter;
    private readonly CultureInfo _culture;

    public CountToVisibilityConverterTests()
    {
        _converter = new CountToVisibilityConverter();
        _culture = CultureInfo.InvariantCulture;
    }

    [Fact]
    public void Convert_WithCountZero_ShouldReturnVisible()
    {
        // Arrange
        int count = 0;

        // Act
        var result = _converter.Convert(count, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithCountZeroAndInvertParameter_ShouldReturnCollapsed()
    {
        // Arrange
        int count = 0;
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(count, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithCountZeroAndInvertParameterCaseInsensitive_ShouldReturnCollapsed()
    {
        // Arrange
        int count = 0;
        string parameter = "invert";

        // Act
        var result = _converter.Convert(count, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithCountPositive_ShouldReturnCollapsed()
    {
        // Arrange
        int count = 5;

        // Act
        var result = _converter.Convert(count, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithCountPositiveAndInvertParameter_ShouldReturnVisible()
    {
        // Arrange
        int count = 5;
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(count, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithCountOne_ShouldReturnCollapsed()
    {
        // Arrange
        int count = 1;

        // Act
        var result = _converter.Convert(count, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithCountOneAndInvertParameter_ShouldReturnVisible()
    {
        // Arrange
        int count = 1;
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(count, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithNegativeCount_ShouldReturnCollapsed()
    {
        // Arrange
        int count = -5;

        // Act
        var result = _converter.Convert(count, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithNegativeCountAndInvertParameter_ShouldReturnVisible()
    {
        // Arrange
        int count = -5;
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(count, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithNonIntValue_ShouldReturnCollapsed()
    {
        // Arrange
        string value = "not an int";

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithNullValue_ShouldReturnCollapsed()
    {
        // Arrange
        object value = null!;

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithStringParameterNotInvert_ShouldNotInvert()
    {
        // Arrange
        int count = 0;
        string parameter = "Other";

        // Act
        var result = _converter.Convert(count, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithEmptyStringParameter_ShouldNotInvert()
    {
        // Arrange
        int count = 0;
        string parameter = "";

        // Act
        var result = _converter.Convert(count, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithLargeCount_ShouldReturnCollapsed()
    {
        // Arrange
        int count = 1000000;

        // Act
        var result = _converter.Convert(count, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithLargeCountAndInvertParameter_ShouldReturnVisible()
    {
        // Arrange
        int count = 1000000;
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(count, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotImplementedException()
    {
        // Arrange
        var value = Visibility.Visible;

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(value, typeof(int), null!, _culture));
    }

    [Fact]
    public void Convert_WithDifferentTargetType_ShouldStillWork()
    {
        // Arrange
        int count = 0;

        // Act
        var result = _converter.Convert(count, typeof(object), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithNullTargetType_ShouldStillWork()
    {
        // Arrange
        int count = 5;
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(count, null!, parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithInt32Value_ShouldWork()
    {
        // Arrange
        int count = 0;

        // Act
        var result = _converter.Convert(count, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithBoxedIntValue_ShouldWork()
    {
        // Arrange
        object count = 0;

        // Act
        var result = _converter.Convert(count, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }
}
