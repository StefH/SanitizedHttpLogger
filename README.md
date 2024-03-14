# SanitizedHttpLogger
Sanitize and reduce the default HttpClient log statements from 4 to 1 per request. This project is based on [MinimalHttpLogger](https://github.com/johnkors/MinimalHttpLogger).

## NuGet
[![NuGet Badge](https://buildstats.info/nuget/SanitizedHttpLogger)](https://www.nuget.org/packages/SanitizedHttpLogger) 

## How does it work

It's not possible to configure the log pattern of the `Microsoft.Extensions.Http` based `HttpClient` loggers. It has to be replaced.

This package replaces the default loggers with a logger that:

1. Reduces the number of log statements on `HttpClient` requests from 4 to 1
2. Logs only 1 aggregated log statement: `{Method} {Uri} - {StatusCode} {StatusCodeLiteral} in {Time}ms`
3. Makes sure that sensitive information like api-keys or tokens in the request-Uri can be sanitized using configurable Regex patterns

### Change in logging output

Before:
```log
info: Start processing HTTP request GET https://my.api.com/v1/status?apikey=my-secret-key
info: Sending HTTP request GET https://my.api.com/v1/status?apikey=my-secret-key
info: Received HTTP response headers after 188.6041ms - 200
info: End processing HTTP request after 188.8026ms - 200
```

After:
```log
info: GET https://my.api.com/v1/status?apikey=xxx - 200 in 186.4883ms
```

## Install

```sh
$ dotnet add package SanitizedHttpLogger
```

## Example Usage

### Via SanitizedHttpLoggerOptions
```csharp
// This regex pattern will match any part of a string that starts with "apikey=" (in a case-insensitive manner) followed by any number of characters that are not an ampersand.
services.UseSanitizedHttpLogger(o => o.RequestUriReplacements.Add("(?i)apikey=[^&]*", "apikey=xxx"));
```

### Via Configuration
``` json
{
  "SanitizedHttpLogger": {
	"RequestUriReplacements": [
	  {
		"Pattern": "(?i)apikey=[^&]*",
		"Replacement": "apikey=xxx"
	  }
	]
  }
}
```

```csharp
services.Configure<SanitizedHttpLoggerOptions>(Configuration);
```	

---

## References
- https://github.com/johnkors/MinimalHttpLogger
- https://josef.codes/customize-the-httpclient-logging-dotnet-core

## Attribution
- <a href="https://www.flaticon.com/free-icons/http" title="http icons">Http icons created by Graphix's Art - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/logs" title="logs icons">Logs icons created by Graphix's Art - Flaticon</a>