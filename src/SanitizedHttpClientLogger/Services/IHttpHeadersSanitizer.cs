using System.Net.Http.Headers;

namespace SanitizedHttpClientLogger.Services;

internal interface IHttpHeadersSanitizer
{
    /// <summary>
    /// Deletes or replaces the given <see cref="HttpHeaders"/> and returns the sanitized header values.
    /// </summary>
    /// <param name="headers">The original <see cref="HttpHeaders"/>.</param>
    /// <returns>A dictionary containing the new header values.</returns>
    IDictionary<string, IEnumerable<string>> Sanitize<T>(T headers) where T: HttpHeaders;
}