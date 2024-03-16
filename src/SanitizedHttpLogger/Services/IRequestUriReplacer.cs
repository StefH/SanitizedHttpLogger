namespace SanitizedHttpLogger.Services;

internal interface IRequestUriReplacer
{
    string? Replace(string? uri);
}