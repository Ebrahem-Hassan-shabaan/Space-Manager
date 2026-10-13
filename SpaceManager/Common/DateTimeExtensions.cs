namespace SpaceManager.Common;

public static class DateTimeExtensions
{
    /// <summary>
    /// Safely converts a UTC DateTime from the database (even if Kind is Unspecified by EF Core) to the local timezone.
    /// </summary>
    public static DateTime ToLocalFromUtc(this DateTime dateTime)
    {
        var utc = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return utc.ToLocalTime();
    }

    /// <summary>
    /// Safely converts a nullable UTC DateTime from the database to the local timezone.
    /// </summary>
    public static DateTime? ToLocalFromUtc(this DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ToLocalFromUtc() : null;
    }

    /// <summary>
    /// Safely converts a user-selected local DateTime to UTC for database storage.
    /// </summary>
    public static DateTime ToUtcFromLocal(this DateTime dateTime)
    {
        var local = dateTime.Kind == DateTimeKind.Local
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        return local.ToUniversalTime();
    }
}
