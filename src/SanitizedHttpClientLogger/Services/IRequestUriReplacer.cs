namespace SanitizedHttpClientLogger.Services;

internal interface IRequestUriReplacer
{
    object? Replace(Uri? uri);
}