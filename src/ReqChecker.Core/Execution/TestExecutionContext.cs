namespace ReqChecker.Core.Execution;

/// <summary>
/// Holds transient execution context for a test, including credentials that should not be serialized.
/// </summary>
public class TestExecutionContext
{
    /// <summary>
    /// Gets or sets the username for the test execution.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for the test execution.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the credential reference ID for looking up credentials from the credential store.
    /// </summary>
    public string? CredentialRef { get; set; }

    /// <summary>
    /// Initializes a new instance of the TestExecutionContext class.
    /// </summary>
    public TestExecutionContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the TestExecutionContext class with credentials.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    public TestExecutionContext(string username, string password)
    {
        Username = username;
        Password = password;
    }

    /// <summary>
    /// Initializes a new instance of the TestExecutionContext class with a credential reference.
    /// </summary>
    /// <param name="credentialRef">The credential reference ID.</param>
    public TestExecutionContext(string credentialRef)
    {
        CredentialRef = credentialRef;
    }

    /// <summary>
    /// Clears sensitive data from the context.
    /// </summary>
    public void ClearSensitiveData()
    {
        Username = null;
        Password = null;
        CredentialRef = null;
    }
}
