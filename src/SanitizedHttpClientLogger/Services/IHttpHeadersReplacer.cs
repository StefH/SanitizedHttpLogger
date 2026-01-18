using System.Collections.Generic;
using System.Net.Http.Headers;

namespace SanitizedHttpClientLogger.Services;

internal interface IHttpHeadersReplacer
{
    /// <summary>
    /// Replaces or sanitizes the given <see cref="HttpHeaders"/> and returns the result.
    /// </summary>
    /// <param name="uri">The original <see cref="HttpHeaders"/> to be replaced or sanitized.</param>
    /// <returns>The sanitized <see cref="HttpHeaders"/>.</returns>
    IDictionary<string, IEnumerable<string>> Replace<T>(T headers) where T: HttpHeaders;
}