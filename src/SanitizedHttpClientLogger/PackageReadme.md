## How does it work
It's not possible to configure the log pattern of the `Microsoft.Extensions.Http` based `HttpClient` loggers. It has to be replaced.

This package replaces the default loggers with a logger that:

1. Reduces the number of log statements on `HttpClient` requests from 4 to 2
2. Makes sure that sensitive information like api-keys or tokens in the request-Uri can be sanitized using configurable Regex patterns

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


### Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **SanitizedHttpClientLogger**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)