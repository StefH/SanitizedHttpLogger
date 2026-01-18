namespace SanitizedHttpClientLogger.Services;

internal interface IUriReplacer
{
    /// <summary>
    /// Replaces or sanitizes the given <see cref="Uri"/> and returns the result.
    /// </summary>
    /// <param name="uri">The original URI to be replaced or sanitized.</param>
    /// <returns>The sanitized Uri, or sanitized string if the Uri cannot be constructed or null if input is null.</returns>
    object? Replace(Uri? uri);
}