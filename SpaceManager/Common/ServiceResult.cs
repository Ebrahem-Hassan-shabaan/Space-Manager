namespace SpaceManager.Common;

/// <summary>
/// Represents the result of a business service operation without throwing exceptions.
/// </summary>
/// <param name="Succeeded">Whether the operation succeeded.</param>
/// <param name="Error">An optional error message describing the failure reason.</param>
/// <param name="IsForbidden">Indicates whether the failure was caused by insufficient permissions (HTTP 403).</param>
public record ServiceResult(bool Succeeded, string? Error = null, bool IsForbidden = false)
{
    /// <summary>
    /// Creates a successful service result.
    /// </summary>
    public static ServiceResult Success() => new(true);

    /// <summary>
    /// Creates a failed service result with an error message (typically HTTP 400).
    /// </summary>
    /// <param name="error">The error message explaining the failure.</param>
    public static ServiceResult Failure(string error) => new(false, Error: error);

    /// <summary>
    /// Creates a forbidden service result representing an authorization violation (HTTP 403).
    /// </summary>
    /// <param name="error">The authorization rejection reason.</param>
    public static ServiceResult Forbidden(string error = "You do not have permission to perform this action.") =>
        new(false, Error: error, IsForbidden: true);
}

/// <summary>
/// Represents the typed result of a business service operation returning data payload upon success.
/// </summary>
/// <typeparam name="T">The type of the returned payload.</typeparam>
/// <param name="Succeeded">Whether the operation succeeded.</param>
/// <param name="Data">The returned data payload upon success; otherwise null.</param>
/// <param name="Error">An optional error message describing the failure reason.</param>
/// <param name="IsForbidden">Indicates whether the failure was caused by insufficient permissions (HTTP 403).</param>
public record ServiceResult<T>(bool Succeeded, T? Data = default, string? Error = null, bool IsForbidden = false)
{
    /// <summary>
    /// Creates a successful typed service result with payload.
    /// </summary>
    /// <param name="data">The returned data payload.</param>
    public static ServiceResult<T> Success(T data) => new(true, Data: data);

    /// <summary>
    /// Creates a failed typed service result with an error message (HTTP 400).
    /// </summary>
    /// <param name="error">The error message explaining the failure.</param>
    public static ServiceResult<T> Failure(string error) => new(false, Error: error);

    /// <summary>
    /// Creates a forbidden typed service result representing an authorization violation (HTTP 403).
    /// </summary>
    /// <param name="error">The authorization rejection reason.</param>
    public static ServiceResult<T> Forbidden(string error = "You do not have permission to perform this action.") =>
        new(false, Error: error, IsForbidden: true);
}
