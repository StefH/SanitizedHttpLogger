using System.Collections.Generic;
using System.Net.Http.Headers;

namespace SanitizedHttpClientLogger.Services;

internal interface IHttpHeadersReplacer
{
    /// <summary>
    /// Replaces or sanitizes the given <see cref="HttpHeaders"/> and returns the sanitized header values.
    /// </summary>
    /// <param name="headers">The original <see cref="HttpHeaders"/> to be replaced or sanitized.</param>
    /// <returns>A dictionary containing the sanitized header values.</returns>
    IDictionary<string, IEnumerable<string>> Replace<T>(T headers) where T: HttpHeaders;
}