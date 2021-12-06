<div align="center">

# DotLog

[![Build Status](https://github.com/riccardotzr/dotlog/workflows/publish/badge.svg)](https://github.com/riccardotzr/ghealth/actions)
[![NuGet Version](https://img.shields.io/nuget/v/DotLogNet.svg)](https://www.nuget.org/packages/DotLogNet/)
[![Coverage Status](https://coveralls.io/repos/github/riccardotzr/dotlog/badge.svg?branch=main)](https://coveralls.io/github/riccardotzr/dotlog?branch=main)

</div>

A .NET 6 Request and Response logging library. It uses [Serilog](https://github.com/serilog/serilog) library and implements a middleware to be used with .NET Core.

## Install
DotLog is installed from NuGet.

```
Install-Package DotLogNet
```

## Usage

The simplest way to set up DotLog is configure the extension method in your application's _Program.cs_.

```csharp
using DotLog;

public class Program 
{
    public stati void Main(string[] args) 
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
            .UseDotLog();
}

```
By default the log level is set to "Information" and the only sink configured is the Console sink. This is because the library is designed to be used in cloud environments, like Kubernetes, in combination with [FluentBit](https://fluentbit.io/) and [Fluentd](https://www.fluentd.org/).

**Then**, the next step is to configure the logging middleware in your application's _Startup.cs_

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
    
    app.UseLoggingMiddleware();
} 
```

There is also an override of the _UseLogging Middleware()_ method that allows you not to log the request and response of certain routes.To do this it is necessary to specify the routes to be excluded as middleware options.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
    
    var routesToBeExcluded = new List<string>() { "/health" };

    app.UseLoggingMiddleware(new LoggerMiddleware() { RoutesToBeExcluded = routesToBeExcluded });
}
```

## Output format

Each request and response log will have the following JSON format:

```json
{
    "Level": "Information",
    "Time": "1611932812",
    "CorrelationId": "29494797-d298-4d33-8229-35a6c28b8948",
    "Message": "My log message",
    "Exception": null,
    "Http": {
        "Request": {
            "Path": "/api/v1/customers/1",
            "Method": "GET",
            "Query": null,
            "Body": null,
            "ContentType": "application/json; charset=utf-8",
            "Scheme": "http",
            "Protocol": "HTTP/1.1",
            "UserAgent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36"
        },
        "Response": {
            "StatusCode": 200,
            "ResponseTime": 0.017,
            "Bytes": null
        }
    },
    "Host": {
        "Hostname": "localhost",
        "ForwardedHostname": "id42.example-cdn.com",
        "Ip": "127.0.0.1"
    }
}
```

**In addition**, the middleware is responsible for recovering the CorrelationId from the HTTP Request Header to use it for each log entry, as follows:

```json
{
    "Level": "Information",
    "Time": "1611932812",
    "CorrelationId": "29494797-d298-4d33-8229-35a6c28b8948",
    "Message": "My other log message",
    "Exception": null
}
```

## Versioning

This project use [SemVer](https://semver.org/) for versioning. For availabe version, see the [tags](https://github.com/riccardotzr/dotlog/tags) on this repository.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE.md](LICENSE.md)
file for details