using System.Globalization;

namespace Sdi_Api.Application.Util;

public static class PeriodParser
{
    public static (DateTime StartDate, DateTime EndDate) ParsePeriod(string periodString,
        DateTime? referenceDate = null)
    {
        var now = referenceDate ?? DateTime.UtcNow;
        DateTime startDate = now;
        DateTime endDate = now;

        periodString = periodString?.ToLowerInvariant() ?? "last30days"; // Default if null

        if (periodString.Contains("_")) // Custom range YYYY-MM-DD_YYYY-MM-DD
        {
            var parts = periodString.Split('_');
            if (parts.Length == 2 &&
                DateTime.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var customStart) &&
                DateTime.TryParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var customEnd))
            {
                return (customStart.Date, customEnd.Date.AddDays(1).AddTicks(-1)); // End of the custom end day
            }

            // Fallback to default if parsing fails
            periodString = "last30days";
        }

        switch (periodString)
        {
            case "last7days":
                startDate = now.Date.AddDays(-6);
                endDate = now.Date.AddDays(1).AddTicks(-1); // End of today
                break;
            case "last30days":
                startDate = now.Date.AddDays(-29);
                endDate = now.Date.AddDays(1).AddTicks(-1);
                break;
            case "last90days":
                startDate = now.Date.AddDays(-89);
                endDate = now.Date.AddDays(1).AddTicks(-1);
                break;
            case "thismonth":
                startDate = new DateTime(now.Year, now.Month, 1);
                endDate = startDate.AddMonths(1).AddTicks(-1);
                break;
            case "lastmonth":
                startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                endDate = startDate.AddMonths(1).AddTicks(-1);
                break;
            case "thisyear":
                startDate = new DateTime(now.Year, 1, 1);
                endDate = new DateTime(now.Year, 12, 31).AddDays(1).AddTicks(-1); // End of Dec 31st
                break;
            case "lastyear":
                startDate = new DateTime(now.Year - 1, 1, 1);
                endDate = new DateTime(now.Year - 1, 12, 31).AddDays(1).AddTicks(-1);
                break;
            default: // Default to last30days if string is unrecognized
                startDate = now.Date.AddDays(-29);
                endDate = now.Date.AddDays(1).AddTicks(-1);
                break;
        }

        return (startDate, endDate);
    }
}
