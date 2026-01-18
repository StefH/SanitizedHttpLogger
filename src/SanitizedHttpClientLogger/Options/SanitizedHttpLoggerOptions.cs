namespace SanitizedHttpClientLogger.Options;

[PublicAPI]
public class SanitizedHttpLoggerOptions
{
    [Obsolete($"Use {nameof(UriReplacements)} instead.")]
    public IDictionary<string, string> RequestUriReplacements
    {
        get => UriReplacements;
        set => UriReplacements = value;
    }

    public IDictionary<string, string> UriReplacements { get; set; } = new Dictionary<string, string>();

    public IDictionary<string, string> HeadersReplacements { get; set; } = new Dictionary<string, string>();
}