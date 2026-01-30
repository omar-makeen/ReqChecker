using System.Windows;
using System.Windows.Controls;
using ReqChecker.Core.Enums;

namespace ReqChecker.App.Controls;

/// <summary>
/// A badge control that displays test status with appropriate styling
/// </summary>
public partial class TestStatusBadge : Border
{
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(TestStatus),
            typeof(TestStatusBadge),
            new PropertyMetadata(TestStatus.Skipped, OnStatusChanged));

    public TestStatus Status
    {
        get => (TestStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TestStatusBadge badge)
        {
            badge.UpdateStatusText();
        }
    }

    private void UpdateStatusText()
    {
        // The TextBlock in the template will bind to Status property
        // This method can be used for additional logic if needed
    }
}
