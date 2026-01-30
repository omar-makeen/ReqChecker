namespace ReqChecker.Core.Enums;

/// <summary>
/// Defines how a field should be treated in the UI.
/// </summary>
public enum FieldPolicyType
{
    /// <summary>
    /// Field is read-only with a lock icon and tooltip explaining it's locked.
    /// </summary>
    Locked,

    /// <summary>
    /// Field is editable as a normal input field.
    /// </summary>
    Editable,

    /// <summary>
    /// Field is not rendered in the UI.
    /// </summary>
    Hidden,

    /// <summary>
    /// Field prompts user for input before test execution.
    /// </summary>
    PromptAtRun
}
