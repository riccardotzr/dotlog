using DotLog;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<LoggingMiddleware>();
            return app;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder app, LoggingMiddlewareOptions options)
        {
            app.UseMiddleware<LoggingMiddleware>(options);
            return app;
        }
    }
}
