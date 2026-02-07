using CommunityToolkit.Mvvm.ComponentModel;
using ReqChecker.Core.Models;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// Wrapper around TestDefinition that adds UI selection state.
/// </summary>
public partial class SelectableTestItem : ObservableObject
{
    /// <summary>
    /// Gets underlying test definition.
    /// </summary>
    public TestDefinition Test { get; }

    /// <summary>
    /// Gets or sets whether this test is selected for the next run.
    /// Default is true (all tests selected by default).
    /// </summary>
    [ObservableProperty]
    private bool _isSelected = true;

    /// <summary>
    /// Gets or sets display text for dependency relationships.
    /// Returns a string like "Depends on: Test A, Test B" or empty string if no dependencies.
    /// </summary>
    public string DependencyDisplayText { get; private set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of SelectableTestItem class.
    /// </summary>
    /// <param name="test">The underlying test definition.</param>
    public SelectableTestItem(TestDefinition test)
    {
        Test = test;
    }

    /// <summary>
    /// Updates dependency display text based on provided profile tests.
    /// </summary>
    /// <param name="profileTests">All tests in the profile to resolve dependency display names.</param>
    public void UpdateDependencyDisplayText(IEnumerable<TestDefinition> profileTests)
    {
        if (Test.DependsOn == null || Test.DependsOn.Count == 0)
        {
            DependencyDisplayText = string.Empty;
            return;
        }

        var displayNames = Test.DependsOn
            .Select(depId => profileTests.FirstOrDefault(t => t.Id == depId)?.DisplayName ?? depId)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        DependencyDisplayText = displayNames.Count > 0
            ? $"Depends on: {string.Join(", ", displayNames)}"
            : string.Empty;
    }
}
