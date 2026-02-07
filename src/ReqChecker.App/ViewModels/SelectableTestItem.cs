using CommunityToolkit.Mvvm.ComponentModel;
using ReqChecker.Core.Models;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// Wrapper around TestDefinition that adds UI selection state.
/// </summary>
public partial class SelectableTestItem : ObservableObject
{
    /// <summary>
    /// Gets the underlying test definition.
    /// </summary>
    public TestDefinition Test { get; }

    /// <summary>
    /// Gets or sets whether this test is selected for the next run.
    /// Default is true (all tests selected by default).
    /// </summary>
    [ObservableProperty]
    private bool _isSelected = true;

    /// <summary>
    /// Initializes a new instance of the SelectableTestItem class.
    /// </summary>
    /// <param name="test">The underlying test definition.</param>
    public SelectableTestItem(TestDefinition test)
    {
        Test = test;
    }
}
