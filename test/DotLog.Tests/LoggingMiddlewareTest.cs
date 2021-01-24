using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Serilog;
using Serilog.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DotLog.Tests
{
    [Collection("Logging Middleware")]
    public class LoggingMiddlewareTest : BaseTest, IClassFixture<DotLogApplicationFactory>
    {
        private const string Url = "/api/v1/customers";
        private const string Scheme = "http";
        private const string ContentType = "application/json; charset=utf-8";
        private const string Protocol = "HTTP/1.1";
        private const string UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
        private const string Hostname = "localhost";
        private const string ForwardedHostname = "id42.example-cdn.com";
        private const string Ip = "127.0.0.1";
        private const int ResponseStatusCode = 200;

        public LoggingMiddlewareTest(DotLogApplicationFactory web) : base(web) { }

        [Fact(DisplayName = "Logging Middleware excluded routes")]
        public async Task Logging_Middleware_Routes_Excluded()
        {
            var url = "/health";

            var (sink, web) = Setup(new LoggingMiddlewareOptions
            {
                RoutesToBeExcluded = new List<string> { url }
            });

            var client = web.CreateClient();

            var result = await client.GetAsync(url);

            var logEvent = sink.Events
                .Where(logEvent => Matching.FromSource<LoggingMiddleware>()(logEvent))
                .ToTestEvent("StatusCode");

            Assert.Null(logEvent);
        }

        [Fact(DisplayName = "LoggingMiddleware without query params")]
        public async Task Logging_Middleware_Get_Request_Without_Query_Params()
        {
            var correlationId = Guid.NewGuid().ToString();
            var method = "GET";
            
            var (sink, web) = Setup();

            var client = web.CreateClient();
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Add("X-Forwarded-Host", ForwardedHostname);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            
            var result = await client.GetAsync(Url);

            var logEvent = sink.Events
                .Where(logEvent => Matching.FromSource<LoggingMiddleware>()(logEvent))
                .ToTestEvent("StatusCode");

            // Events
            Assert.NotEmpty(sink.Events);

            // Request
            Assert.Equal(method, logEvent.Properties["Method"].LiteralValue());
            Assert.Equal(Url, logEvent.Properties["Url"].LiteralValue());
            Assert.Null(logEvent.Properties["RequestBody"].LiteralValue());
            Assert.Null(logEvent.Properties["Query"].LiteralValue());
            Assert.Null(logEvent.Properties["ContentType"].LiteralValue());
            Assert.Equal(correlationId, logEvent.Properties["CorrelationId"].LiteralValue());
            Assert.Equal(Scheme, logEvent.Properties["Scheme"].LiteralValue());
            Assert.Equal(Protocol, logEvent.Properties["Protocol"].LiteralValue());
            Assert.Equal(UserAgent, logEvent.Properties["UserAgent"].LiteralValue());

            // Host
            Assert.Equal(Hostname, logEvent.Properties["Hostname"].LiteralValue());
            Assert.Equal(ForwardedHostname, logEvent.Properties["ForwardedHostname"].LiteralValue());
            Assert.Equal(Ip, logEvent.Properties["Ip"].LiteralValue());

            // Response
            Assert.Equal(ResponseStatusCode, logEvent.Properties["StatusCode"].LiteralValue());
            Assert.NotNull(logEvent.Properties["ResponseTime"].LiteralValue());
        }

        [Fact(DisplayName = "LoggingMiddleware with query params")]
        public async Task Logging_Middleware_Get_Request_With_Query_Params()
        {
            var correlationId = Guid.NewGuid().ToString();
            var urlQuery = "/api/v1/customers/1?Name=Foo";
            var method = "GET";

            var (sink, web) = Setup();

            var client = web.CreateClient();
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Add("X-Forwarded-Host", ForwardedHostname);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

            var result = await client.GetAsync(urlQuery);

            var logEvent = sink.Events
                .Where(logEvent => Matching.FromSource<LoggingMiddleware>()(logEvent))
                .ToTestEvent("StatusCode");

            // Events
            Assert.NotEmpty(sink.Events);

            // Request
            Assert.Equal(method, logEvent.Properties["Method"].LiteralValue());
            Assert.Equal("/api/v1/customers/1", logEvent.Properties["Url"].LiteralValue());
            Assert.Null(logEvent.Properties["RequestBody"].LiteralValue());
            Assert.NotNull(logEvent.Properties["Query"]);
            Assert.Null(logEvent.Properties["ContentType"].LiteralValue());
            Assert.Equal(correlationId, logEvent.Properties["CorrelationId"].LiteralValue());
            Assert.Equal(Scheme, logEvent.Properties["Scheme"].LiteralValue());
            Assert.Equal(Protocol, logEvent.Properties["Protocol"].LiteralValue());
            Assert.Equal(UserAgent, logEvent.Properties["UserAgent"].LiteralValue());

            // Host
            Assert.Equal(Hostname, logEvent.Properties["Hostname"].LiteralValue());
            Assert.Equal(ForwardedHostname, logEvent.Properties["ForwardedHostname"].LiteralValue());
            Assert.Equal(Ip, logEvent.Properties["Ip"].LiteralValue());

            // Response
            Assert.Equal(ResponseStatusCode, logEvent.Properties["StatusCode"].LiteralValue());
            Assert.NotNull(logEvent.Properties["ResponseTime"].LiteralValue());
        }

        [Fact(DisplayName = "LoggingMiddleware request body")]
        public async Task Logging_Middleware_Post_Request()
        {
            var correlationId = Guid.NewGuid().ToString();
            var method = "POST";

            var (sink, web) = Setup();

            var client = web.CreateClient();
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Add("X-Forwarded-Host", ForwardedHostname);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

            var bodyContent = new StringContent(JsonConvert.SerializeObject(new { Id = 1 }), Encoding.UTF8, "application/json");
            var result = await client.PostAsync(Url, bodyContent);

            var logEvent = sink.Events
                .Where(logEvent => Matching.FromSource<LoggingMiddleware>()(logEvent))
                .ToTestEvent("StatusCode");

            // Events
            Assert.NotEmpty(sink.Events);

            // Request
            Assert.Equal(method, logEvent.Properties["Method"].LiteralValue());
            Assert.Equal(Url, logEvent.Properties["Url"].LiteralValue());
            Assert.NotNull(logEvent.Properties["RequestBody"].LiteralValue());
            Assert.Equal(JsonConvert.SerializeObject(new { Id = 1 }), logEvent.Properties["RequestBody"].LiteralValue());
            Assert.Null(logEvent.Properties["Query"].LiteralValue());
            Assert.Equal(ContentType, logEvent.Properties["ContentType"].LiteralValue());
            Assert.Equal(correlationId, logEvent.Properties["CorrelationId"].LiteralValue());
            Assert.Equal(Scheme, logEvent.Properties["Scheme"].LiteralValue());
            Assert.Equal(Protocol, logEvent.Properties["Protocol"].LiteralValue());
            Assert.Equal(UserAgent, logEvent.Properties["UserAgent"].LiteralValue());

            // Host
            Assert.Equal(Hostname, logEvent.Properties["Hostname"].LiteralValue());
            Assert.Equal(ForwardedHostname, logEvent.Properties["ForwardedHostname"].LiteralValue());
            Assert.Equal(Ip, logEvent.Properties["Ip"].LiteralValue());

            // Response
            Assert.Equal(ResponseStatusCode, logEvent.Properties["StatusCode"].LiteralValue());
            Assert.NotNull(logEvent.Properties["ResponseTime"].LiteralValue());
        }
    }
}
