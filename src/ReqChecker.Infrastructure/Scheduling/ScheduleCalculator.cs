using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;

namespace ReqChecker.Infrastructure.Scheduling;

/// <summary>
/// Calculates the next run time for a schedule and formats human-readable recurrence descriptions.
/// </summary>
public class ScheduleCalculator
{
    /// <summary>
    /// Calculates the next run time for a schedule based on its type and recurrence pattern.
    /// Returns null for one-time schedules (they don't repeat) or completed/expired schedules.
    /// </summary>
    public DateTime? CalculateNextRunTime(Schedule schedule, DateTime? fromTime = null)
    {
        var now = fromTime ?? DateTime.Now;

        if (schedule.ScheduleType == ScheduleType.OneTime)
        {
            // One-time: only valid if still in the future
            return schedule.ScheduledTime > now ? schedule.ScheduledTime : null;
        }

        if (schedule.Recurrence == null) return null;

        var recurrence = schedule.Recurrence;

        // Check end date
        if (recurrence.EndDate.HasValue && now.Date > recurrence.EndDate.Value.Date)
            return null;

        return recurrence.FrequencyType switch
        {
            RecurrenceType.Hourly => CalculateHourlyNext(recurrence, now),
            RecurrenceType.Daily => CalculateDailyNext(recurrence, now),
            RecurrenceType.Weekly => CalculateWeeklyNext(recurrence, now),
            RecurrenceType.CustomInterval => CalculateCustomIntervalNext(recurrence, schedule.LastRunTime ?? schedule.ScheduledTime, now),
            _ => null
        };
    }

    private static DateTime CalculateHourlyNext(RecurrencePattern recurrence, DateTime now)
    {
        var intervalHours = recurrence.IntervalValue < 1 ? 1 : recurrence.IntervalValue;
        var candidate = now.AddHours(intervalHours);
        return new DateTime(candidate.Year, candidate.Month, candidate.Day, candidate.Hour, 0, 0);
    }

    private static DateTime CalculateDailyNext(RecurrencePattern recurrence, DateTime now)
    {
        var timeOfDay = recurrence.TimeOfDay;
        var candidate = now.Date.Add(timeOfDay);

        // If today's time has passed, move to tomorrow
        if (candidate <= now)
            candidate = candidate.AddDays(1);

        return candidate;
    }

    private static DateTime CalculateWeeklyNext(RecurrencePattern recurrence, DateTime now)
    {
        if (recurrence.DaysOfWeek == null || recurrence.DaysOfWeek.Count == 0)
            return now.AddDays(7);

        var timeOfDay = recurrence.TimeOfDay;

        // Try up to 7 days ahead to find the next matching day
        for (int daysAhead = 0; daysAhead <= 7; daysAhead++)
        {
            var candidate = now.Date.AddDays(daysAhead).Add(timeOfDay);
            if (recurrence.DaysOfWeek.Contains(candidate.DayOfWeek) && candidate > now)
                return candidate;
        }

        // Fallback: same time next week on first selected day
        return now.AddDays(7);
    }

    private static DateTime CalculateCustomIntervalNext(RecurrencePattern recurrence, DateTime lastRun, DateTime now)
    {
        var intervalValue = recurrence.IntervalValue < 1 ? 1 : recurrence.IntervalValue;
        var unit = recurrence.IntervalUnit ?? Core.Enums.IntervalUnit.Minutes;

        DateTime next = unit switch
        {
            Core.Enums.IntervalUnit.Minutes => lastRun.AddMinutes(intervalValue),
            Core.Enums.IntervalUnit.Hours => lastRun.AddHours(intervalValue),
            Core.Enums.IntervalUnit.Days => lastRun.AddDays(intervalValue),
            _ => lastRun.AddMinutes(intervalValue)
        };

        // If computed next is already in the past, advance from now
        if (next <= now)
        {
            next = unit switch
            {
                Core.Enums.IntervalUnit.Minutes => now.AddMinutes(intervalValue),
                Core.Enums.IntervalUnit.Hours => now.AddHours(intervalValue),
                Core.Enums.IntervalUnit.Days => now.AddDays(intervalValue),
                _ => now.AddMinutes(intervalValue)
            };
        }

        return next;
    }

    /// <summary>
    /// Returns a human-readable description of the recurrence pattern.
    /// Examples: "Every Monday at 9:00 AM", "Every 2 hours", "Every day at 10:00 PM"
    /// </summary>
    public string GetRecurrenceDescription(Schedule schedule)
    {
        if (schedule.ScheduleType == ScheduleType.OneTime)
            return "One-time";

        if (schedule.Recurrence == null)
            return "Recurring";

        var r = schedule.Recurrence;

        return r.FrequencyType switch
        {
            RecurrenceType.Hourly when r.IntervalValue == 1 => "Every hour",
            RecurrenceType.Hourly => $"Every {r.IntervalValue} hours",
            RecurrenceType.Daily => $"Every day at {FormatTime(r.TimeOfDay)}",
            RecurrenceType.Weekly => FormatWeekly(r),
            RecurrenceType.CustomInterval => FormatCustomInterval(r),
            _ => "Recurring"
        };
    }

    private static string FormatTime(TimeSpan time)
    {
        var dt = DateTime.Today.Add(time);
        return dt.ToString("h:mm tt");
    }

    private static string FormatWeekly(RecurrencePattern r)
    {
        if (r.DaysOfWeek == null || r.DaysOfWeek.Count == 0)
            return $"Weekly at {FormatTime(r.TimeOfDay)}";

        if (r.DaysOfWeek.Count == 1)
            return $"Every {r.DaysOfWeek[0]} at {FormatTime(r.TimeOfDay)}";

        // Sort days
        var sortedDays = r.DaysOfWeek
            .OrderBy(d => ((int)d + 6) % 7) // Monday = 0 ordering
            .Select(d => d.ToString().Substring(0, 3))
            .ToList();

        return $"Every {string.Join(", ", sortedDays)} at {FormatTime(r.TimeOfDay)}";
    }

    private static string FormatCustomInterval(RecurrencePattern r)
    {
        var unit = r.IntervalUnit switch
        {
            Core.Enums.IntervalUnit.Minutes => r.IntervalValue == 1 ? "minute" : "minutes",
            Core.Enums.IntervalUnit.Hours => r.IntervalValue == 1 ? "hour" : "hours",
            Core.Enums.IntervalUnit.Days => r.IntervalValue == 1 ? "day" : "days",
            _ => "minutes"
        };
        return $"Every {r.IntervalValue} {unit}";
    }
}
