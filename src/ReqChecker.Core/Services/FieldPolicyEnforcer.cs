using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Text.Json.Nodes;

namespace ReqChecker.Core.Services;

/// <summary>
/// Applies field-level policies to test definitions.
/// </summary>
public static class FieldPolicyEnforcer
{
    /// <summary>
    /// Applies field-level policies to a test definition based on its field policies.
    /// </summary>
    /// <param name="test">The test definition to apply policies to.</param>
    /// <returns>A new test definition with policies applied.</returns>
    public static TestDefinition ApplyPolicies(TestDefinition test)
    {
        var result = new TestDefinition
        {
            Id = test.Id,
            Type = test.Type,
            DisplayName = test.DisplayName,
            Description = test.Description,
            Parameters = new JsonObject(),
            FieldPolicy = new Dictionary<string, FieldPolicyType>(test.FieldPolicy),
            Timeout = test.Timeout,
            RetryCount = test.RetryCount,
            RequiresAdmin = test.RequiresAdmin
        };

        // Copy parameters
        foreach (var param in test.Parameters)
        {
            result.Parameters[param.Key] = param.Value?.DeepClone();
        }

        // Apply field policies to parameters
        foreach (var policyEntry in test.FieldPolicy)
        {
            var fieldPath = policyEntry.Key;
            var policy = policyEntry.Value;

            // For Locked fields, mark them as read-only in metadata
            if (policy == FieldPolicyType.Locked)
            {
                // Store policy metadata in the parameter object
                if (result.Parameters[fieldPath] is JsonObject paramObj)
                {
                    paramObj["isReadOnly"] = true;
                }
            }
            // For Hidden fields, mark them as not visible
            else if (policy == FieldPolicyType.Hidden)
            {
                if (result.Parameters[fieldPath] is JsonObject paramObj)
                {
                    paramObj["isVisible"] = false;
                }
            }
            // PromptAtRun will be handled at runtime during test execution
        }

        return result;
    }
}
