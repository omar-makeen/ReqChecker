using System.Text.Json;
using System.Text.Json.Serialization;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using Serilog;

namespace ReqChecker.Infrastructure.Scheduling;

/// <summary>
/// Persists schedule definitions and execution records to %APPDATA%/ReqChecker/schedules.json.
/// </summary>
public class SchedulePersistenceService : ISchedulePersistenceService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public SchedulePersistenceService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "ReqChecker");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "schedules.json");
    }

    public async Task<List<Schedule>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return (await LoadStoreAsync()).Schedules;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveScheduleAsync(Schedule schedule)
    {
        await _lock.WaitAsync();
        try
        {
            var store = await LoadStoreAsync();
            var existing = store.Schedules.FindIndex(s => s.Id == schedule.Id);
            if (existing >= 0)
                store.Schedules[existing] = schedule;
            else
                store.Schedules.Add(schedule);

            await WriteStoreAsync(store);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteScheduleAsync(string scheduleId)
    {
        await _lock.WaitAsync();
        try
        {
            var store = await LoadStoreAsync();
            store.Schedules.RemoveAll(s => s.Id == scheduleId);
            store.ExecutionRecords.RemoveAll(r => r.ScheduleId == scheduleId);
            await WriteStoreAsync(store);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveExecutionRecordAsync(ScheduleExecutionRecord record)
    {
        await _lock.WaitAsync();
        try
        {
            var store = await LoadStoreAsync();
            store.ExecutionRecords.Add(record);
            // Keep only the last 500 records to avoid unbounded growth
            if (store.ExecutionRecords.Count > 500)
                store.ExecutionRecords = store.ExecutionRecords.TakeLast(500).ToList();
            await WriteStoreAsync(store);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<ScheduleExecutionRecord>> GetExecutionRecordsAsync(string scheduleId)
    {
        await _lock.WaitAsync();
        try
        {
            var store = await LoadStoreAsync();
            return store.ExecutionRecords
                .Where(r => r.ScheduleId == scheduleId)
                .OrderByDescending(r => r.ExecutionTime)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    // ── private helpers ──────────────────────────────────────────────

    private async Task<ScheduleStore> LoadStoreAsync()
    {
        if (!File.Exists(_filePath))
            return new ScheduleStore();

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<ScheduleStore>(json, JsonOptions) ?? new ScheduleStore();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load schedules.json; starting fresh");
            return new ScheduleStore();
        }
    }

    private async Task WriteStoreAsync(ScheduleStore store)
    {
        var json = JsonSerializer.Serialize(store, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}

/// <summary>Root object stored in schedules.json.</summary>
internal class ScheduleStore
{
    public int Version { get; set; } = 1;
    public List<Schedule> Schedules { get; set; } = new();
    public List<ScheduleExecutionRecord> ExecutionRecords { get; set; } = new();
}
