using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLog.Tests
{
    public class TextWriteSink : ILogEventSink
    {
        private readonly StringWriter _output;
        private readonly ITextFormatter _formatter;

        public TextWriteSink(StringWriter output, ITextFormatter formatter)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public void Emit(LogEvent logEvent)
        {
            _formatter.Format(logEvent, _output);
        }
    }
}
