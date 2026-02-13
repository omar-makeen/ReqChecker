using ReqChecker.App.ViewModels;
using System.Windows;

namespace ReqChecker.App.Views;

/// <summary>
/// Dialog for prompting credentials during test execution.
/// </summary>
public partial class CredentialPromptDialog : Window
{
    private readonly CredentialPromptViewModel _viewModel;

    public CredentialPromptDialog(CredentialPromptViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Wire button events explicitly to avoid relying on generated XAML event hookups.
        SubmitButton.Click += SubmitButton_Click;
        CancelButton.Click += CancelButton_Click;
    }

    /// <summary>
    /// Gets the username entered by the user.
    /// </summary>
    public string? Username => _viewModel.Username;

    /// <summary>
    /// Gets the password entered by the user.
    /// </summary>
    public string? Password => _viewModel.Password;

    /// <summary>
    /// Gets whether the user chose to remember credentials.
    /// </summary>
    public bool RememberCredentials => _viewModel.RememberCredentials;

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.PasswordBox passwordBox)
        {
            _viewModel.Password = passwordBox.Password;
        }
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        // Sync password from PasswordBox (it doesn't support binding)
        _viewModel.Password = PasswordBox.Password;

        // Validate password is not empty
        if (string.IsNullOrWhiteSpace(_viewModel.Password))
        {
            _viewModel.ErrorMessage = "Password is required.";
            return;
        }

        // Clear error and close
        _viewModel.ErrorMessage = null;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
