using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using DotLog;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseDotLog(this IHostBuilder builder)
        {
            builder.UseSerilog((context, provider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .WriteTo.Console(new DotLogJsonFormatter());
            });

            return builder;
        }
    }
}
