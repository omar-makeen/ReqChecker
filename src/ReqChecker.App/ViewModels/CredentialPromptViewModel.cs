using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// ViewModel for prompting credentials during test execution.
/// </summary>
public partial class CredentialPromptViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _fieldName;

    [ObservableProperty]
    private string? _fieldLabel;

    [ObservableProperty]
    private string? _credentialRef;

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private bool _rememberCredentials;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Gets or sets the callback to invoke when credentials are submitted.
    /// </summary>
    public Action<string?, string?>? OnCredentialsSubmitted { get; set; }

    /// <summary>
    /// Gets or sets the callback to invoke when the dialog is cancelled.
    /// </summary>
    public Action? OnCancelled { get; set; }

    /// <summary>
    /// Initializes the ViewModel with field information.
    /// </summary>
    public void Initialize(string fieldName, string fieldLabel, string? credentialRef = null)
    {
        FieldName = fieldName;
        FieldLabel = fieldLabel;
        CredentialRef = credentialRef;
        Username = string.Empty;
        Password = string.Empty;
        RememberCredentials = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Validates the credentials and submits them.
    /// </summary>
    [RelayCommand]
    private void Submit()
    {
        // Validate username is not empty
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required.";
            return;
        }

        // Validate password is not empty
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required.";
            return;
        }

        // Clear error and submit
        ErrorMessage = string.Empty;
        OnCredentialsSubmitted?.Invoke(Username, Password);
    }

    /// <summary>
    /// Cancels the credential prompt.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        ErrorMessage = string.Empty;
        OnCancelled?.Invoke();
    }
}
