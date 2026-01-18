using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SanitizedHttpClientLogger.Options;

namespace SanitizedHttpClientLogger.Services;

internal class UriReplacer : IUriReplacer
{
    private readonly ConcurrentDictionary<Lazy<Regex>, string> _uriReplacements = new();

    public UriReplacer(IOptions<SanitizedHttpLoggerOptions> options)
    {
        var requestUriReplacements = Guard.NotNull(options.Value).UriReplacements;
        foreach (var replacement in requestUriReplacements)
        {
            _uriReplacements.TryAdd(new Lazy<Regex>(() => new Regex(replacement.Key, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100))), replacement.Value);
        }
    }

    public object? Replace(Uri? uri)
    {
        if (uri == null)
        {
            return null;
        }

        var replaced = _uriReplacements.Aggregate(uri.ToString(), (current, item) => item.Key.Value.Replace(current, item.Value));

        try
        {
            return new Uri(replaced);
        }
        catch
        {
            // In case the Uri is invalid, return the replaced value as string.
            return replaced;
        }
    }
}