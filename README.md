# SanitizedHttpLogger
Sanitize and reduce the default HttpClient log statements from 4 to 1 per request. This project is based on

## NuGet
todo

## How does it work

It's not possible to configure the log pattern of the `Microsoft.Extensions.Http` based `HttpClient` loggers. To modify, one has to replace them. This package replaces the default loggers with a logger that:

1. Reduces the number of log statements on httpclient requests from 4 to 1
2. Logs 1 aggregated log statement: `{Method} {Uri} - {StatusCode} {StatusCodeLiteral} in {Time}ms`
3. Makes sure that sensitive information like api-keys or tokens in the request-Uri can be sanitized using configurable regex patterns


### Change in output

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

## Usage

```csharp
services.UseMinimalHttpLogger();
```

---

## :books: References
- https://github.com/johnkors/MinimalHttpLogger/tree/main
- https://josef.codes/customize-the-httpclient-logging-dotnet-core/
