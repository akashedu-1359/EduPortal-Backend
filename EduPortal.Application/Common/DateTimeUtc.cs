namespace EduPortal.Application.Common;

public static class DateTimeUtc
{
    /// <summary>
    /// Ensures timestamps are UTC for PostgreSQL timestamptz columns.
    /// </summary>
    public static DateTime? Normalize(DateTime? value)
    {
        if (!value.HasValue) return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
        };
    }
}
