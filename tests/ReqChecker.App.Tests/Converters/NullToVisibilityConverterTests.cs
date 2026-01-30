using System.Globalization;
using ReqChecker.App.Converters;
using System.Windows;

namespace ReqChecker.App.Tests.Converters;

/// <summary>
/// Unit tests for NullToVisibilityConverter.
/// </summary>
public class NullToVisibilityConverterTests
{
    private readonly NullToVisibilityConverter _converter;
    private readonly CultureInfo _culture;

    public NullToVisibilityConverterTests()
    {
        _converter = new NullToVisibilityConverter();
        _culture = CultureInfo.InvariantCulture;
    }

    [Fact]
    public void Convert_WithNonNullObject_ShouldReturnVisible()
    {
        // Arrange
        object value = new object();

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithNonNullString_ShouldReturnVisible()
    {
        // Arrange
        string value = "test";

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithEmptyString_ShouldReturnCollapsed()
    {
        // Arrange
        string value = "";

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithWhitespaceString_ShouldReturnVisible()
    {
        // Arrange
        string value = "   ";

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithZeroInt_ShouldReturnVisible()
    {
        // Arrange
        int value = 0;

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithFalseBool_ShouldReturnVisible()
    {
        // Arrange
        bool value = false;

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithTrueBool_ShouldReturnVisible()
    {
        // Arrange
        bool value = true;

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithDifferentTargetType_ShouldStillWork()
    {
        // Arrange
        object value = new object();

        // Act
        var result = _converter.Convert(value, typeof(object), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithNullTargetType_ShouldStillWork()
    {
        // Arrange
        int value = 5;
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(value, null!, parameter, _culture);

        // Assert - Non-null value with Invert parameter returns Collapsed
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithInvertParameterAndNullTargetType_ShouldStillWork()
    {
        // Arrange
        object value = new object();
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(value, null!, parameter, _culture);

        // Assert - Non-null value with Invert parameter returns Collapsed
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithStringParameterNotInvert_ShouldNotInvert()
    {
        // Arrange
        object value = new object();
        string parameter = "Other";

        // Act
        var result = _converter.Convert(value, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithEmptyStringParameter_ShouldNotInvert()
    {
        // Arrange
        object value = new object();
        string parameter = "";

        // Act
        var result = _converter.Convert(value, typeof(Visibility), parameter, _culture);

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
            _converter.ConvertBack(value, typeof(string), null!, _culture));
    }

    [Fact]
    public void Convert_WithDBNull_ShouldReturnVisible()
    {
        // Arrange
        object value = DBNull.Value;

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithDBNullAndInvertParameter_ShouldReturnCollapsed()
    {
        // Arrange
        object value = DBNull.Value;
        string parameter = "Invert";

        // Act
        var result = _converter.Convert(value, typeof(Visibility), parameter, _culture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithGuidValue_ShouldReturnVisible()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithEmptyGuid_ShouldReturnVisible()
    {
        // Arrange
        Guid value = Guid.Empty;

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithDateTimeValue_ShouldReturnVisible()
    {
        // Arrange
        DateTime value = DateTime.Now;

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithDefaultDateTime_ShouldReturnVisible()
    {
        // Arrange
        DateTime value = default;

        // Act
        var result = _converter.Convert(value, typeof(Visibility), null!, _culture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }
}
