namespace SanitizedHttpClientLogger.Services;

internal interface IRequestUriReplacer
{
    string? Replace(string? uri);
}