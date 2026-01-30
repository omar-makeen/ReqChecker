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

        // Set up callbacks
        _viewModel.OnCredentialsSubmitted = (username, password) =>
        {
            DialogResult = true;
            Close();
        };

        _viewModel.OnCancelled = () =>
        {
            DialogResult = false;
            Close();
        };
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
}
