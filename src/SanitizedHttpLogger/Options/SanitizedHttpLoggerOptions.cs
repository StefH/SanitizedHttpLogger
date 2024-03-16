using JetBrains.Annotations;

namespace SanitizedHttpLogger.Options;

[PublicAPI]
public class SanitizedHttpLoggerOptions
{
    public IDictionary<string, string> RequestUriReplacements { get; set; } = new Dictionary<string, string>();
}