using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SanitizedHttpClientLogger.Options;

namespace SanitizedHttpClientLogger.Services;

internal class HttpHeadersReplacer : IHttpHeadersReplacer
{
    private readonly ConcurrentDictionary<Lazy<Regex>, string> _headerValuesReplacements = new();

    public HttpHeadersReplacer(IOptions<SanitizedHttpLoggerOptions> options)
    {
        var requestUriReplacements = Guard.NotNull(options.Value).HeadersReplacements;
        foreach (var replacement in requestUriReplacements)
        {
            _headerValuesReplacements.TryAdd(new Lazy<Regex>(() => new Regex(replacement.Key, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100))), replacement.Value);
        }
    }

    public IDictionary<string, IEnumerable<string>> Replace<T>(T headers) where T : HttpHeaders
    {
        var headersCopy = new Dictionary<string, IEnumerable<string>>();
                
        foreach (var header in headers.ToArray())
        {
            var match = _headerValuesReplacements.FirstOrDefault(h => h.Key.Value.IsMatch(header.Key));

            if (!match.Equals(default(KeyValuePair<Lazy<Regex>, string>)))
            {
                headersCopy.Add(header.Key, [match.Value]);
                continue;
            }
            
            headersCopy.Add(header.Key, header.Value);
        }        

        return headersCopy;
    }
}