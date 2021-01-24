using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLog.Tests
{
    public static class Extensions
    {
        public static object LiteralValue(this LogEventPropertyValue @this)
        {
            return ((ScalarValue)@this).Value;
        }

        public static LogEvent ToTestEvent(this IEnumerable<LogEvent> events, string propertyKey)
        {
            foreach (var @event in events)
            {
                var properties = @event.Properties;

                if (properties.ContainsKey(propertyKey))
                {
                    return @event;
                }
            }

            return null;
        }
    }
}
