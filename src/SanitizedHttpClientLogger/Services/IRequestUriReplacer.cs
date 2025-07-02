namespace SanitizedHttpClientLogger.Services;

internal interface IRequestUriReplacer
{
    Uri? Replace(Uri? uri);
}