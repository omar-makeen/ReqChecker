using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;

namespace ReqChecker.App.ViewModels;

public partial class TestConfigViewModel : ObservableObject
{
    private readonly TestDefinition _testDefinition;

    [ObservableProperty]
    private string _testName = string.Empty;

    [ObservableProperty]
    private string _testType = string.Empty;

    [ObservableProperty]
    private bool _requiresAdmin;

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
    public ICommand CancelCommand { get; }
    public ICommand PromptForCredentialsCommand { get; }

    public TestConfigViewModel(TestDefinition testDefinition)
    {
        _testDefinition = testDefinition ?? throw new ArgumentNullException(nameof(testDefinition));
        TestName = testDefinition.DisplayName;
        TestType = testDefinition.Type;
        RequiresAdmin = testDefinition.RequiresAdmin;
        Timeout = testDefinition.Timeout;
        RetryCount = testDefinition.RetryCount;
        InitializeParameters();
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(OnCancel);
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
        if (!Parameters.Any(p => p.Name == "Timeout"))
        {
            Parameters.Add(new TestParameterViewModel("Timeout", Timeout?.ToString() ?? "30000", FieldPolicyType.Editable));
        }
        if (!Parameters.Any(p => p.Name == "RetryCount"))
        {
            Parameters.Add(new TestParameterViewModel("RetryCount", RetryCount?.ToString() ?? "3", FieldPolicyType.Editable));
        }
        if (!Parameters.Any(p => p.Name == "RequiresAdmin"))
        {
            Parameters.Add(new TestParameterViewModel("RequiresAdmin", RequiresAdmin.ToString().ToLower(), FieldPolicyType.Locked));
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
                if (param.Name == "Timeout" && int.TryParse(param.Value, out int timeout))
                {
                    _testDefinition.Timeout = timeout;
                }
                else if (param.Name == "RetryCount" && int.TryParse(param.Value, out int retryCount))
                {
                    _testDefinition.RetryCount = retryCount;
                }
                else if (param.Name == "RequiresAdmin" && bool.TryParse(param.Value, out bool requiresAdmin))
                {
                    _testDefinition.RequiresAdmin = requiresAdmin;
                }
                else
                {
                    _testDefinition.Parameters[param.Name] = param.Value;
                }
            }
            await Task.Delay(100);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void OnCancel()
    {
        TestName = _testDefinition.DisplayName;
        RequiresAdmin = _testDefinition.RequiresAdmin;
        Timeout = _testDefinition.Timeout;
        RetryCount = _testDefinition.RetryCount;
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

    public TestParameterViewModel(string name, string value, FieldPolicyType policy)
    {
        Name = name;
        Value = value;
        Policy = policy;
        IsLocked = policy == FieldPolicyType.Locked || policy == FieldPolicyType.Hidden;
    }
}
