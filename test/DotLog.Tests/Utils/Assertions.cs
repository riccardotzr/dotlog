using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLog.Tests
{
    public static class Assertions
    {
        public static JObject AssertValidJson(ITextFormatter formatter, Action<ILogger> action)
        {
            var output = new StringWriter();
            var log = new LoggerConfiguration()
                .WriteTo.Sink(new TextWriteSink(output, formatter))
                .CreateLogger();

            action(log);

            var json = output.ToString();

            return JsonConvert.DeserializeObject<JObject>(json, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            });
        }
    }
}
