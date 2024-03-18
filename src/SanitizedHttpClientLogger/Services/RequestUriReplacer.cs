using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SanitizedHttpLogger.Options;

namespace SanitizedHttpClientLogger.Services;

internal class RequestUriReplacer : IRequestUriReplacer
{
    private readonly ConcurrentDictionary<Lazy<Regex>, string> _requestUriReplacements = new();

    public RequestUriReplacer(IOptions<SanitizedHttpLoggerOptions> options)
    {
        var requestUriReplacements = Guard.NotNull(options.Value).RequestUriReplacements;
        foreach (var replacement in requestUriReplacements)
        {
            _requestUriReplacements.TryAdd(new Lazy<Regex>(() => new Regex(replacement.Key, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100))), replacement.Value);
        }
    }

    public string? Replace(string? uri)
    {
        return uri == null ? null : _requestUriReplacements.Aggregate(uri, (current, item) => item.Key.Value.Replace(current, item.Value));
    }
}