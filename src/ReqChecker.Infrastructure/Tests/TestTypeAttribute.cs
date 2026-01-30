namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Attribute to mark test implementations for DI registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TestTypeAttribute : Attribute
{
    /// <summary>
    /// The test type identifier (e.g., "HttpGet", "Ping").
    /// </summary>
    public string TypeIdentifier { get; }

    /// <summary>
    /// Initializes a new instance of the TestType attribute.
    /// </summary>
    /// <param name="typeIdentifier">The test type identifier.</param>
    public TestTypeAttribute(string typeIdentifier)
    {
        TypeIdentifier = typeIdentifier;
    }
}
