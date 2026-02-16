using System.Globalization;

namespace EmployeesOverlap.Core;

public static class CsvLoader
{
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "dd-MM-yyyy",
        "dd/MM/yyyy",
        "MM-dd-yyyy",
        "MM/dd/yyyy",
        "d-M-yyyy",
        "d/M/yyyy",
        "M-d-yyyy",
        "M/d/yyyy",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy/MM/dd HH:mm:ss",
        "dd/MM/yyyy HH:mm:ss",
        "MM/dd/yyyy HH:mm:ss"
    ];

    public static IReadOnlyList<WorkRecord> Load(string filePath, DateTime? todayOverride = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("CSV file not found.", filePath);

        var today = (todayOverride ?? DateTime.Today).Date;

        var lines = File.ReadAllLines(filePath);
        var result = new List<WorkRecord>(capacity: Math.Max(16, lines.Length));

        for (int i = 0; i < lines.Length; i++)
        {
            var raw = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            // Optional header row
            if (i == 0 && raw.StartsWith("EmpID", StringComparison.OrdinalIgnoreCase))
                continue;

            var parts = raw.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 4)
                throw new FormatException($"Invalid CSV format at line {i + 1}: '{lines[i]}'");

            if (!int.TryParse(parts[0], out var empId))
                throw new FormatException($"Invalid EmpID at line {i + 1}: '{parts[0]}'");

            if (!int.TryParse(parts[1], out var projectId))
                throw new FormatException($"Invalid ProjectID at line {i + 1}: '{parts[1]}'");

            var dateFrom = ParseDate(parts[2], i + 1);
            var dateTo = ParseDateTo(parts[3], today, i + 1);

            // Ignore invalid ranges
            if (dateTo < dateFrom)
                continue;

            result.Add(new WorkRecord(empId, projectId, dateFrom, dateTo));
        }

        return result;
    }

    private static DateTime ParseDate(string input, int lineNo)
    {
        input = input.Trim();

        if (DateTime.TryParseExact(input, DateFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var dtExact))
            return dtExact.Date;

        // Broad parsing (helps with multiple formats)
        if (DateTime.TryParse(input, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var dtAny))
            return dtAny.Date;

        if (DateTime.TryParse(input, CultureInfo.CurrentCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var dtLocal))
            return dtLocal.Date;

        throw new FormatException($"Unrecognized date at line {lineNo}: '{input}'");
    }

    private static DateTime ParseDateTo(string input, DateTime today, int lineNo)
    {
        input = input.Trim();

        if (string.IsNullOrEmpty(input) || input.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return today;

        return ParseDate(input, lineNo);
    }
}
