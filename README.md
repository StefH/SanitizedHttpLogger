# SanitizedHttpLogger & SanitizedHttpClientLogger
Sanitize and reduce the default HttpClient log statements from 4 to 1 per request. This project is based on [MinimalHttpLogger](https://github.com/johnkors/MinimalHttpLogger).

## NuGet
| Name | NuGet | Frameworks |
| - | - | - |
| SanitizedHttpClientLogger | [![NuGet Badge](https://buildstats.info/nuget/SanitizedHttpClientLogger)](https://www.nuget.org/packages/SanitizedHttpClientLogger) | <ul><li>`.NET 8` and up</li></ul>
| SanitizedHttpLogger | [![NuGet Badge](https://buildstats.info/nuget/SanitizedHttpLogger)](https://www.nuget.org/packages/SanitizedHttpLogger) | <ul><li>`.NET Framework 4.8`</li><li>`.NET Standard 2.0`</li><li>`.NET Standard 2.1`</li><li>`.NET 6.0` and up</li></ul>


## How does it work
It's not possible to configure the log pattern of the `Microsoft.Extensions.Http` based `HttpClient` loggers. It has to be replaced.

This package replaces the default loggers with a logger that:

1. Reduces the number of log statements on `HttpClient` requests from 4 to 2
2. Logs only 2 log statement
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
info: Sending HTTP request GET https://my.api.com/v1/status?apikey=xxx
info: Received HTTP response GET https://my.api.com/v1/status?apikey=xxx - 200 in 186.4883ms
```

---

## :bulb: SanitizedHttpClientLogger

### Install

```sh
$ dotnet add package SanitizedHttpClientLogger
```

### Usage

#### Via Options
```csharp
services
    .AddHttpClient<Worker>((_, o) =>
    {
        o.BaseAddress = new Uri("https://my.api.com/v1");
    })
    .ConfigureSanitizedLogging(o =>
        // This regex pattern will match any part of a string that starts with "apikey=" (in a case-insensitive manner)
        // followed by any number of characters that are not an ampersand.
        o.RequestUriReplacements.Add("(?i)apikey=[^&]*", "apikey=xxx")
    );
```

#### Via Configuration
```json
{
  "SanitizedHttpLoggerOptions": {
    "RequestUriReplacements": {
      "(?i)apikey=[^&]*": "apikey=xxx"
    }
  }
}
```

```csharp
services
    .AddHttpClient<Worker>((_, o) =>
    {
        o.BaseAddress = new Uri("https://my.api.com/v1");
    })
    .ConfigureSanitizedLogging(configuration);
```

---

## :bulb: SanitizedHttpLogger

### Install

```sh
$ dotnet add package SanitizedHttpLogger
```

### Usage

#### Via Options
```csharp
// This regex pattern will match any part of a string that starts with "apikey=" (in a case-insensitive manner)
// followed by any number of characters that are not an ampersand.
services.UseSanitizedHttpLogger(o => 
    o.RequestUriReplacements.Add("(?i)apikey=[^&]*", "apikey=xxx")
);
```

#### Via Configuration
```json
{
  "SanitizedHttpLoggerOptions": {
    "RequestUriReplacements": {
      "(?i)apikey=[^&]*": "apikey=xxx"
    }
  }
}
```

```csharp
services.Configure<SanitizedHttpLoggerOptions>(Configuration);
```

---

## :books: References
- https://github.com/johnkors/MinimalHttpLogger
- https://josef.codes/customize-the-httpclient-logging-dotnet-core


## :clap: Attribution
- <a href="https://www.flaticon.com/free-icons/http" title="http icons">Http icons created by Graphix's Art - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/logs" title="logs icons">Logs icons created by Graphix's Art - Flaticon</a>