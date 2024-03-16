using JetBrains.Annotations;

namespace SanitizedHttpLogger.Options;

[PublicAPI]
public class SanitizedHttpLoggerOptions
{
    public Dictionary<string, string> RequestUriReplacements { get; set; } = new();
}