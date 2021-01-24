using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotLog.Tests
{
    public class BaseTest
    {
        private readonly DotLogApplicationFactory _web;

        public BaseTest(DotLogApplicationFactory web)
        {
            _web = web ?? throw new ArgumentNullException(nameof(web));
        }

        protected (EventLogSink, WebApplicationFactory<TestStartup>) Setup()
        {
            var sink = new EventLogSink();
            var web = _web.WithWebHostBuilder(builder => builder
                .Configure(app =>
                {   
                    app.Use(async (context, next) =>
                    {
                        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                        await next();
                    });
                    app.UseLoggingMiddleware();
                    app.Run(_ => Task.CompletedTask);
                })
                .UseSerilog((builder, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .MinimumLevel.Information()
                        .WriteTo.Console(new DotLogJsonFormatter())
                        .WriteTo.Sink(sink);
                }));

            return (sink, web);
        }

        protected (EventLogSink, WebApplicationFactory<TestStartup>) Setup(LoggingMiddlewareOptions options) 
        {
            var sink = new EventLogSink();
            var web = _web.WithWebHostBuilder(builder => builder
                .Configure(app =>
                {
                    app.Use(async (context, next) =>
                    {
                        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                        await next();
                    });
                    app.UseLoggingMiddleware(options);
                    app.Run(_ => Task.CompletedTask);
                })
                .UseSerilog((builder, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .MinimumLevel.Information()
                        .WriteTo.Console(new DotLogJsonFormatter())
                        .WriteTo.Sink(sink);
                }));

            return (sink, web);
        }
    }
}
