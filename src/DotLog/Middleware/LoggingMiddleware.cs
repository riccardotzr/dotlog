using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLog
{
    /// <summary>
    /// HTTP Request and Response Logging Middleware
    /// </summary>
    public class LoggingMiddleware
    {
        private const string UserAgentHeaderKey = "User-Agent";
        private const string ForwardedHostHeaderKey = "x-forwarded-host";
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";

        private readonly RequestDelegate _next;
        private readonly LoggingMiddlewareOptions _options;
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly ConcurrentDictionary<string, object> _loggingDictionary;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggingDictionary = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public LoggingMiddleware(RequestDelegate next, LoggingMiddlewareOptions options, ILogger<LoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggingDictionary = new ConcurrentDictionary<string, object>();
        }

        ///<inheritdoc/>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;

            if (_options != null && _options.RoutesToBeExcluded.Any() && _options.RoutesToBeExcluded.Contains(path))
            {
                await _next(httpContext);
            }
            else
            {
                var responseTimeWatch = new Stopwatch();
                responseTimeWatch.Start();

                var correlationId = httpContext.Request.Headers[CorrelationIdHeaderKey].ToString();

                await LogIncomingRequest(httpContext);

                using (_logger.BeginScope(new Dictionary<string, object>() { { "CorrelationId", correlationId } }))
                {
                    await _next(httpContext);
                }

                await LogCompletedRequest(httpContext, responseTimeWatch);
            }
        }

        private async Task LogIncomingRequest(HttpContext httpContext)
        {
            var request = httpContext.Request;

            var hasBody = request.ContentLength > 0;
            var body = string.Empty;

            if (hasBody)
            {
                body = await GetRequestBody(request);
            }

            var query = request.Query.ToList();
            var forwardedHostname = request.Headers[ForwardedHostHeaderKey].ToString();
            var userAgent = request.Headers[UserAgentHeaderKey].ToString();
            var correlationId = GetOrCreateCorrelationId(request);

            _loggingDictionary.TryAdd("HasHttpRequest", true);
            _loggingDictionary.TryAdd("CorrelationId", correlationId);
            _loggingDictionary.TryAdd("Method", request.Method.ToString());
            _loggingDictionary.TryAdd("Url", request.Path);
            _loggingDictionary.TryAdd("RequestBody", (hasBody) ? body : null);
            _loggingDictionary.TryAdd("Query", (query != null && query.Count > 0) ? query : null);
            _loggingDictionary.TryAdd("Scheme", request.Scheme);
            _loggingDictionary.TryAdd("ContentType", request.ContentType);
            _loggingDictionary.TryAdd("Protocol", request.Protocol);
            _loggingDictionary.TryAdd("Ip", httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString());
            _loggingDictionary.TryAdd("Hostname", RemovePort(request.Host.ToString()));
            _loggingDictionary.TryAdd("ForwardedHostname", (!string.IsNullOrEmpty(forwardedHostname)) ? forwardedHostname : null);
            _loggingDictionary.TryAdd("UserAgent", (!string.IsNullOrEmpty(userAgent)) ? userAgent : null);

            using (_logger.BeginScope(_loggingDictionary))
            {
                _logger.LogInformation("Incoming Request");
            }

        }

        private async Task LogCompletedRequest(HttpContext httpContext, Stopwatch responseTimeWatch)
        {   
            var response = httpContext.Response;

            var responseBody = string.Empty;
            var responseBytes = default(long);

            var originalResponseBody = response.Body;

            using (var responseBodyStream = new MemoryStream())
            {
                response.Body = responseBodyStream;

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                responseBody = new StreamReader(responseBodyStream).ReadToEnd();
                responseBodyStream.Seek(0, SeekOrigin.Begin);

                responseBytes = response.ContentLength ?? responseBodyStream.Length;

                await responseBodyStream.CopyToAsync(originalResponseBody);
            }

            responseTimeWatch.Stop();
            var passedMicroSeconds = responseTimeWatch.ElapsedMilliseconds / 1000m;

            _loggingDictionary.TryAdd("HasHttpResponse", true);
            _loggingDictionary.TryAdd("StatusCode", httpContext.Response.StatusCode);
            _loggingDictionary.TryAdd("ResponseTime", passedMicroSeconds);
            _loggingDictionary.TryAdd("ResponseBytes", responseBytes);


            using (_logger.BeginScope(_loggingDictionary))
            {
                _logger.LogInformation("Completed Request");
            }

            _loggingDictionary.Clear();
        }

        private static string GetOrCreateCorrelationId(HttpRequest request)
        {
            var hasCorrelationId = request.Headers.TryGetValue(CorrelationIdHeaderKey, out var cid);
            var correlationId = hasCorrelationId ? cid.FirstOrDefault() : null;

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                request.Headers.Add(CorrelationIdHeaderKey, correlationId);
            }

            return correlationId;
        }

        private static async Task<string> GetRequestBody(HttpRequest request)
        {
            var result = string.Empty;

            request.EnableBuffering();

            using (var buffer = new MemoryStream())
            {
                request.Body.Seek(0, SeekOrigin.Begin);

                await request.Body.CopyToAsync(buffer);
                result = Encoding.UTF8.GetString(buffer.ToArray());

                request.Body.Seek(0, SeekOrigin.Begin);
            }

            return result;
        }

        private static string RemovePort(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return null;
            }

            var hostComponents = host.Split(":");
            return hostComponents[0];
        }

    }
}
