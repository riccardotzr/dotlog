using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLog.Tests
{
    public class EventLogSink : ILogEventSink
    {
        public List<LogEvent> Events { get; set; } = new List<LogEvent>();

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}
