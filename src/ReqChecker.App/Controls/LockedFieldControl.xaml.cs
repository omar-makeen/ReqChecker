using System.Windows;
using System.Windows.Controls;

namespace ReqChecker.App.Controls;

/// <summary>
/// Control for displaying locked (read-only) fields with a lock icon.
/// </summary>
public partial class LockedFieldControl : UserControl
{
    /// <summary>
    /// Gets or sets the value to display.
    /// </summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Dependency property for Value.
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(LockedFieldControl),
            new PropertyMetadata(string.Empty));

    public LockedFieldControl()
    {
        InitializeComponent();
    }
}
