using System.Windows;
using System.Windows.Controls;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// View for test configuration with field-level policy enforcement.
/// </summary>
public partial class TestConfigView
{
    public TestConfigView(TestConfigViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>
    /// Handles PasswordChanged event for password-type parameters.
    /// Syncs the PasswordBox value back to the TestParameterViewModel.Value property.
    /// </summary>
    private void ParameterPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && passwordBox.DataContext is TestParameterViewModel parameterViewModel)
        {
            parameterViewModel.Value = passwordBox.Password;
        }
    }
}
