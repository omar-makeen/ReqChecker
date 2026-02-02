using ReqChecker.Core.Models;
using Serilog;
using System.IO;
using System.Text.Json;

namespace ReqChecker.Infrastructure.History;

/// <summary>
/// Service for history operations with JSON persistence.
/// </summary>
public class HistoryService : IHistoryService
{
    private readonly string _historyPath;
    private readonly string _backupPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<RunReport> _history;
    private bool _loaded;

    /// <summary>
    /// Initializes a new instance of the HistoryService class.
    /// </summary>
    public HistoryService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var historyDir = Path.Combine(appDataPath, "ReqChecker");
        
        _historyPath = Path.Combine(historyDir, "history.json");
        _backupPath = Path.Combine(historyDir, "history.json.backup");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        _history = new List<RunReport>();
        _loaded = false;
    }

    /// <summary>
    /// Load history from disk (call on startup).
    /// </summary>
    /// <returns>List of historical runs.</returns>
    public async Task<List<RunReport>> LoadHistoryAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_loaded)
            {
                return _history.ToList();
            }

            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(_historyPath))
                    {
                        var json = File.ReadAllText(_historyPath);
                        var store = JsonSerializer.Deserialize<HistoryStore>(json, _jsonOptions);

                        if (store != null && store.Runs != null)
                        {
                            _history = store.Runs;
                            Log.Information("Loaded {RunCount} historical runs from {HistoryPath}", _history.Count, _historyPath);
                        }
                    }
                    else
                    {
                        _history = new List<RunReport>();
                        Log.Information("No history file found at {HistoryPath}, starting with empty history", _historyPath);
                    }
                }
                catch (JsonException ex)
                {
                    Log.Warning(ex, "Failed to parse history file at {HistoryPath}, attempting backup", _historyPath);

                    // Try to restore from backup
                    if (File.Exists(_backupPath))
                    {
                        try
                        {
                            var backupJson = File.ReadAllText(_backupPath);
                            var store = JsonSerializer.Deserialize<HistoryStore>(backupJson, _jsonOptions);

                            if (store != null && store.Runs != null)
                            {
                                _history = store.Runs;
                                Log.Information("Restored {RunCount} runs from backup", _history.Count);
                            }
                        }
                        catch (Exception backupEx)
                        {
                            Log.Error(backupEx, "Failed to restore from backup, starting with empty history");
                            _history = new List<RunReport>();
                        }
                    }
                    else
                    {
                        Log.Warning("No backup available, starting with empty history");
                        _history = new List<RunReport>();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load history, starting with empty history");
                    _history = new List<RunReport>();
                }
            });

            _loaded = true;
            return _history.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Ensures history is loaded (thread-safe).
    /// </summary>
    private async Task EnsureLoadedAsync()
    {
        if (!_loaded)
        {
            await LoadHistoryAsync();
        }
    }

    /// <summary>
    /// Save a new run to history (call after test completion).
    /// </summary>
    /// <param name="report">The run report to save.</param>
    public async Task SaveRunAsync(RunReport report)
    {
        if (report == null)
        {
            throw new ArgumentNullException(nameof(report));
        }

        await EnsureLoadedAsync();

        await _lock.WaitAsync();
        try
        {
            // Add the new run
            _history.Add(report);

            // Sort by start time (newest first)
            _history = _history.OrderByDescending(r => r.StartTime).ToList();

            // Save to file
            await Task.Run(() => SaveToFile());

            Log.Information("Saved run {RunId} to history", report.RunId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save run {RunId} to history", report.RunId);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Delete a specific run.
    /// </summary>
    /// <param name="runId">The run ID to delete.</param>
    public async Task DeleteRunAsync(string runId)
    {
        if (string.IsNullOrEmpty(runId))
        {
            throw new ArgumentException("Run ID cannot be null or empty", nameof(runId));
        }

        await EnsureLoadedAsync();

        await _lock.WaitAsync();
        try
        {
            var initialCount = _history.Count;
            _history = _history.Where(r => r.RunId != runId).ToList();

            if (_history.Count < initialCount)
            {
                await Task.Run(() => SaveToFile());
                Log.Information("Deleted run {RunId} from history", runId);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete run {RunId} from history", runId);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Clear all history.
    /// </summary>
    public async Task ClearHistoryAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _history = new List<RunReport>();
            await Task.Run(() => SaveToFile());

            Log.Information("Cleared all history");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to clear history");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Get storage statistics.
    /// </summary>
    /// <returns>Storage statistics.</returns>
    public HistoryStats GetStats()
    {
        _lock.Wait();
        try
        {
            if (!_loaded || _history.Count == 0)
            {
                return HistoryStats.Empty;
            }

            long fileSizeBytes = 0;
            if (File.Exists(_historyPath))
            {
                try
                {
                    fileSizeBytes = new FileInfo(_historyPath).Length;
                }
                catch
                {
                    // Ignore file size errors
                }
            }

            return new HistoryStats
            {
                TotalRuns = _history.Count,
                FileSizeBytes = fileSizeBytes,
                OldestRun = _history.Min(r => r.StartTime),
                NewestRun = _history.Max(r => r.StartTime)
            };
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Saves the current history to file atomically.
    /// </summary>
    private void SaveToFile()
    {
        var directory = Path.GetDirectoryName(_historyPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create backup if file exists
        if (File.Exists(_historyPath))
        {
            try
            {
                File.Copy(_historyPath, _backupPath, overwrite: true);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to create backup of history file");
            }
        }

        var store = new HistoryStore
        {
            Version = "1.0",
            LastUpdated = DateTimeOffset.UtcNow,
            Runs = _history
        };

        var json = JsonSerializer.Serialize(store, _jsonOptions);
        File.WriteAllText(_historyPath, json);
    }
}
