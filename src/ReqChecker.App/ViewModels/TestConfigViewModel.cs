using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using ReqChecker.App.Services;

namespace ReqChecker.App.ViewModels;

public partial class TestConfigViewModel : ObservableObject
{
    private readonly TestDefinition _testDefinition;
    private readonly NavigationService _navigationService;
    private readonly DialogService _dialogService;
    private Dictionary<string, string?> _baseline = new();

    [ObservableProperty]
    private string _testName = string.Empty;

    [ObservableProperty]
    private string _testType = string.Empty;

    [ObservableProperty]
    private bool _requiresAdmin;

    public string RequiresAdminText => RequiresAdmin ? "Yes" : "No";

    partial void OnRequiresAdminChanged(bool value)
    {
        OnPropertyChanged(nameof(RequiresAdminText));
    }

    [ObservableProperty]
    private int? _timeout;

    [ObservableProperty]
    private int? _retryCount;

    [ObservableProperty]
    private bool _showCredentialPrompt;

    [ObservableProperty]
    private string _credentialPromptText = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    public ObservableCollection<TestParameterViewModel> Parameters { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand PromptForCredentialsCommand { get; }

    public bool HasUnsavedChanges
    {
        get
        {
            if (Timeout?.ToString() != _baseline.GetValueOrDefault("Timeout"))
                return true;
            if (RetryCount?.ToString() != _baseline.GetValueOrDefault("RetryCount"))
                return true;
            
            foreach (var param in Parameters.Where(p => p.IsEditable || p.IsPassword))
            {
                if (param.Value != _baseline.GetValueOrDefault(param.Name))
                    return true;
            }
            
            return false;
        }
    }

    public TestConfigViewModel(TestDefinition testDefinition, NavigationService navigationService, DialogService dialogService)
    {
        _testDefinition = testDefinition ?? throw new ArgumentNullException(nameof(testDefinition));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        TestName = testDefinition.DisplayName;
        TestType = testDefinition.Type;
        RequiresAdmin = testDefinition.RequiresAdmin;
        Timeout = testDefinition.Timeout;
        RetryCount = testDefinition.RetryCount;
        InitializeParameters();
        CaptureBaseline();
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        BackCommand = new RelayCommand(OnBack);
        PromptForCredentialsCommand = new RelayCommand(OnPromptForCredentials);
    }

    private void InitializeParameters()
    {
        if (_testDefinition.Parameters != null)
        {
            foreach (var param in _testDefinition.Parameters)
            {
                var policy = GetFieldPolicy(param.Key);
                Parameters.Add(new TestParameterViewModel(param.Key, param.Value?.ToString() ?? string.Empty, policy));
            }
        }
    }

    private FieldPolicyType GetFieldPolicy(string fieldName)
    {
        if (_testDefinition.FieldPolicy != null && _testDefinition.FieldPolicy.TryGetValue(fieldName, out var policy))
        {
            return policy;
        }
        return FieldPolicyType.Editable;
    }

    private void CaptureBaseline()
    {
        _baseline = new Dictionary<string, string?>
        {
            ["Timeout"] = Timeout?.ToString(),
            ["RetryCount"] = RetryCount?.ToString()
        };
        
        foreach (var param in Parameters.Where(p => p.IsEditable || p.IsPassword))
        {
            _baseline[param.Name] = param.Value;
        }
    }

    private async Task SaveAsync()
    {
        IsSaving = true;
        try
        {
            _testDefinition.RequiresAdmin = RequiresAdmin;
            _testDefinition.Timeout = Timeout;
            _testDefinition.RetryCount = RetryCount;
            foreach (var param in Parameters)
            {
                _testDefinition.Parameters[param.Name] = param.Value;
            }
            await Task.Delay(100);
            CaptureBaseline();
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void OnBack()
    {
        if (HasUnsavedChanges)
        {
            var discard = _dialogService.ShowConfirmationDialog(
                "Unsaved Changes",
                "You have unsaved changes. Do you want to discard them?");
            
            if (!discard)
            {
                return;
            }
        }
        
        _navigationService.GoBack();
    }

    private void OnPromptForCredentials()
    {
        ShowCredentialPrompt = true;
        CredentialPromptText = "Enter credentials for: " + TestName;
    }

    public void CloseCredentialPrompt()
    {
        ShowCredentialPrompt = false;
    }
}

public partial class TestParameterViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private FieldPolicyType _policy;

    [ObservableProperty]
    private bool _isLocked;

    public bool IsHidden => Policy == FieldPolicyType.Hidden;
    public bool IsEditable => Policy == FieldPolicyType.Editable;
    public bool IsPromptAtRun => Policy == FieldPolicyType.PromptAtRun;
    public string Label => Name;

    /// <summary>
    /// Indicates whether this parameter is a password field (should be masked in UI).
    /// True when the parameter name ends with "Password" (case-insensitive).
    /// </summary>
    public bool IsPassword => Name.EndsWith("Password", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Inverse of IsPassword for XAML visibility binding.
    /// </summary>
    public bool IsNotPassword => !IsPassword;

    public TestParameterViewModel(string name, string value, FieldPolicyType policy)
    {
        Name = name;
        Value = value;
        Policy = policy;
        IsLocked = policy == FieldPolicyType.Locked || policy == FieldPolicyType.Hidden;
    }
}
