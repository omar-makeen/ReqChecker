using CommunityToolkit.Mvvm.ComponentModel;

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
}
