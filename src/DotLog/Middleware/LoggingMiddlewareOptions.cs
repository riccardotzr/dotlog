using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLog
{
    /// <summary>
    /// Options to configure LoggingMiddleware
    /// </summary>
    public class LoggingMiddlewareOptions
    {
        /// <summary>
        /// Indicates the routes that should not be logged.
        /// </summary>
        public List<string> RoutesToBeExcluded { get; set; }
    }
}
