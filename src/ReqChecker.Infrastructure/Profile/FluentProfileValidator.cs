using FluentValidation;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.ProfileManagement;

/// <summary>
/// Validates profile structure and content using FluentValidation.
/// </summary>
public class FluentProfileValidator : IProfileValidator
{
    private readonly ProfileValidator _validator;

    public FluentProfileValidator()
    {
        _validator = new ProfileValidator();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> ValidateAsync(ProfileModel profile)
    {
        if (profile == null)
        {
            return new[] { "Profile cannot be null." };
        }

        var result = await _validator.ValidateAsync(profile);
        return result.Errors.Select(e => $"[{e.PropertyName}] {e.ErrorMessage}");
    }

    /// <summary>
    /// FluentValidation rules for Profile.
    /// </summary>
    private class ProfileValidator : AbstractValidator<ProfileModel>
    {
        public ProfileValidator()
        {
            RuleFor(p => p.Id)
                .NotEmpty().WithMessage("Profile ID is required.")
                .Must(BeValidGuid).WithMessage("Profile ID must be a valid GUID.");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Profile name is required.")
                .Length(1, 100).WithMessage("Profile name must be between 1 and 100 characters.");

            RuleFor(p => p.SchemaVersion)
                .GreaterThanOrEqualTo(1).WithMessage("Schema version must be at least 1.")
                .LessThanOrEqualTo(10).WithMessage("Schema version must not exceed 10.");

            RuleFor(p => p.Tests)
                .NotEmpty().WithMessage("Profile must contain at least one test.")
                .Must(HaveUniqueTestIds).WithMessage("All test IDs within a profile must be unique.");

            RuleForEach(p => p.Tests)
                .SetValidator(new TestDefinitionValidator());

            RuleFor(p => p.RunSettings)
                .NotNull().WithMessage("Run settings are required.")
                .SetValidator(new RunSettingsValidator());
        }

        private static bool BeValidGuid(string id)
        {
            return Guid.TryParse(id, out _);
        }

        private static bool HaveUniqueTestIds(List<TestDefinition> tests)
        {
            if (tests == null || tests.Count == 0)
            {
                return true;
            }

            return tests.Select(t => t.Id).Distinct().Count() == tests.Count;
        }
    }

    /// <summary>
    /// FluentValidation rules for TestDefinition.
    /// </summary>
    private class TestDefinitionValidator : AbstractValidator<TestDefinition>
    {
        public TestDefinitionValidator()
        {
            RuleFor(t => t.Id)
                .NotEmpty().WithMessage("Test ID is required.");

            RuleFor(t => t.Type)
                .NotEmpty().WithMessage("Test type is required.");

            RuleFor(t => t.DisplayName)
                .NotEmpty().WithMessage("Display name is required.");

            RuleFor(t => t.Timeout)
                .GreaterThanOrEqualTo(1000).When(t => t.Timeout.HasValue)
                .WithMessage("Timeout must be at least 1000ms.")
                .LessThanOrEqualTo(300000).When(t => t.Timeout.HasValue)
                .WithMessage("Timeout must not exceed 300000ms.");

            RuleFor(t => t.RetryCount)
                .GreaterThanOrEqualTo(0).When(t => t.RetryCount.HasValue)
                .WithMessage("Retry count must be non-negative.")
                .LessThanOrEqualTo(10).When(t => t.RetryCount.HasValue)
                .WithMessage("Retry count must not exceed 10.");
        }
    }

    /// <summary>
    /// FluentValidation rules for RunSettings.
    /// </summary>
    private class RunSettingsValidator : AbstractValidator<RunSettings>
    {
        public RunSettingsValidator()
        {
            RuleFor(r => r.DefaultTimeout)
                .InclusiveBetween(1000, 300000)
                .WithMessage("Default timeout must be between 1000 and 300000ms.");

            RuleFor(r => r.DefaultRetryCount)
                .InclusiveBetween(0, 10)
                .WithMessage("Default retry count must be between 0 and 10.");
        }
    }
}
