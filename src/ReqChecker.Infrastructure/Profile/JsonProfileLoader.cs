using ReqChecker.Core.Interfaces;
using ProfileModel = ReqChecker.Core.Models.Profile;
using System.Text.Json;

namespace ReqChecker.Infrastructure.ProfileManagement;

/// <summary>
/// Loads profiles from JSON files or embedded resources.
/// </summary>
public class JsonProfileLoader : IProfileLoader
{
    private readonly JsonSerializerOptions _options;

    public JsonProfileLoader()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    /// <inheritdoc/>
    public async Task<ProfileModel> LoadFromFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Profile file not found: {filePath}", filePath);
        }

        await using var stream = File.OpenRead(filePath);
        return await LoadFromStreamAsync(stream);
    }

    /// <inheritdoc/>
    public async Task<ProfileModel> LoadFromStreamAsync(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var profile = await JsonSerializer.DeserializeAsync<ProfileModel>(stream, _options)
            ?? throw new InvalidOperationException("Failed to deserialize profile from stream.");

        return profile;
    }
}
