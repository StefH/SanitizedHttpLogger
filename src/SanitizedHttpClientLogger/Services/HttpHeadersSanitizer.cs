using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SanitizedHttpClientLogger.Options;

namespace SanitizedHttpClientLogger.Services;

internal class HttpHeadersSanitizer : IHttpHeadersSanitizer
{
    private readonly Dictionary<Lazy<Regex>, string> _headerValuesReplacements = [];
    private readonly IList<Lazy<Regex>> _headersDeletions = [];

    public HttpHeadersSanitizer(IOptions<SanitizedHttpLoggerOptions> options)
    {
        foreach (var replacement in Guard.NotNull(options.Value).HeadersReplacements)
        {
            _headerValuesReplacements.Add(new Lazy<Regex>(() => new Regex(replacement.Key, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100))), replacement.Value);
        }

        foreach (var deletion in Guard.NotNull(options.Value).HeadersDeletions)
        {
            _headersDeletions.Add(new Lazy<Regex>(() => new Regex(deletion, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100))));
        }
    }

    public IDictionary<string, IEnumerable<string>> Sanitize<T>(T headers) where T : HttpHeaders
    {
        var headersCopy = headers
            .Where(h => !_headersDeletions.Any(d => d.Value.IsMatch(h.Key)))
            .ToDictionary(h => h.Key, h => h.Value);
                
        foreach (var header in headers.ToArray())
        {
            var match = _headerValuesReplacements.FirstOrDefault(h => h.Key.Value.IsMatch(header.Key));

            if (!match.Equals(default(KeyValuePair<Lazy<Regex>, string>)))
            {
                headersCopy[header.Key] = [match.Value];
                continue;
            }
        }        

        return headersCopy;
    }
}