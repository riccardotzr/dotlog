using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DotLog.Tests
{
    [Collection("Json Formatter")]
    public class JsonFormatterTest
    {
        public static JObject AssertValidJson(Action<ILogger> action)
        {
            return Assertions.AssertValidJson(new DotLogJsonFormatter(), action);
        }

        [Theory(DisplayName = "Log Message must be the same")]
        [InlineData("Incoming Request")]
        public void Formatter_Message_Be_The_Same(string expectedValue)
        {
            var json = AssertValidJson(log => log.Information(expectedValue));

            Assert.True(json.TryGetValue("Message", out var value));
            Assert.Equal(expectedValue, value);
        }

        [Fact(DisplayName = "Log Level must be the same")]
        public void Formatter_Level_Be_The_Same()
        {
            string expectedLevel = "Information";
            var json = AssertValidJson(log => log.Information("Incoming Request"));

            Assert.True(json.TryGetValue("Level", out var value));
            Assert.Equal(expectedLevel, value);
        }

        [Fact(DisplayName = "Log CorrelationId is null")]
        public void Formatter_CorrelationId_Is_Null()
        {
            var json = AssertValidJson(log => log.Information("An Empty CorrelationId"));

            Assert.True(json.TryGetValue("CorrelationId", out var value));
            Assert.Equal("null", value);
        }

        [Fact(DisplayName = "Log Timestamp is not null")]
        public void Formatter_Timestamp_Is_Not_Null()
        {
            var json = AssertValidJson(log => log.Information("Timestamp is not null"));

            Assert.True(json.TryGetValue("Time", out var value));
            Assert.NotNull(value);
        }

        [Fact(DisplayName = "Log Exception is null")]
        public void Formatter_Exception_Is_Null()
        {
            var json = AssertValidJson(log => log.Information("Exception is null"));

            Assert.True(json.TryGetValue("Exception", out var value));
            Assert.Null(value.Value<Exception>());
        }

        [Fact(DisplayName = "Log Exception is not null")]
        public void Formmater_Exception_Is_Not_Null()
        {
            var exception = new Exception("Fake Error");

            var json = AssertValidJson(log => log.Error(exception, "Exception is null"));

            Assert.True(json.TryGetValue("Exception", out var value));
            Assert.NotNull(value);
        }

        [Fact(DisplayName = "Log HTTP GET Request info is not null")]
        public void Formatter_Http_Get_Request_Context_Info_Is_Not_Null()
        {
            var correlationId = Guid.NewGuid().ToString();
            var hasHttpRequest = true;
            var url = "/api/v1/customers/1";
            var method = "GET";
            var contentType = "application/json";
            var scheme = "http";
            var protocol = "HTTP/1.1";
            var userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            var hostname = "localhost";
            var forwardedHostname = "id42.example-cdn.com";
            var ip = "127.0.0.1";
            var hasHttpResponse = true;
            var statusCode = 200;
            var responseTime = 1500000;
            var responseBytes = 10;

            var json = AssertValidJson(log => log
                .ForContext("CorrelationId", correlationId)
                .ForContext("HasHttpRequest", hasHttpRequest)
                .ForContext("Url", url)
                .ForContext("Method", method)
                .ForContext("ContentType", contentType)
                .ForContext("Scheme", scheme)
                .ForContext("Protocol", protocol)
                .ForContext("UserAgent", userAgent)
                .ForContext("HasHttpResponse", hasHttpResponse)
                .ForContext("StatusCode", statusCode)
                .ForContext("ResponseTime", responseTime)
                .ForContext("ResponseBytes", responseBytes)
                .ForContext("Bytes", responseBytes)
                .ForContext("Hostname", hostname)
                .ForContext("ForwardedHostname", forwardedHostname)
                .ForContext("Ip", ip)
                .Information("Completed Request"));

            var actualCorrelationId = json["CorrelationId"];
            var actualUrl = json["Http"]["Request"]["Path"];
            var actualMethod = json["Http"]["Request"]["Method"];
            var actualContentType = json["Http"]["Request"]["ContentType"];
            var actualScheme = json["Http"]["Request"]["Scheme"];
            var actualProtocol = json["Http"]["Request"]["Protocol"];
            var actualUserAgent = json["Http"]["Request"]["UserAgent"];
            var actualStatusCode = json["Http"]["Response"]["StatusCode"];
            var actualResponseTime = json["Http"]["Response"]["ResponseTime"];
            var actualResponseBytes = json["Http"]["Response"]["Bytes"];
            var actualHostname = json["Host"]["Hostname"];
            var actualForwardedHostname = json["Host"]["ForwardedHostname"];
            var actualIp = json["Host"]["Ip"];

            Assert.NotNull(actualCorrelationId);
            Assert.Equal(correlationId, actualCorrelationId.Value<string>());

            Assert.NotNull(actualUrl);
            Assert.Equal(url, actualUrl.Value<string>());

            Assert.NotNull(actualMethod);
            Assert.Equal(method, actualMethod.Value<string>());

            Assert.NotNull(actualContentType);
            Assert.Equal(contentType, actualContentType.Value<string>());

            Assert.NotNull(actualScheme);
            Assert.Equal(scheme, actualScheme.Value<string>());

            Assert.NotNull(actualProtocol);
            Assert.Equal(protocol, actualProtocol.Value<string>());

            Assert.NotNull(actualUserAgent);
            Assert.Equal(userAgent, actualUserAgent.Value<string>());

            Assert.NotNull(actualStatusCode);
            Assert.Equal(statusCode, actualStatusCode);

            Assert.NotNull(actualResponseTime);
            Assert.Equal(responseTime, actualResponseTime);

            Assert.NotNull(actualResponseBytes);
            Assert.Equal(responseBytes, actualResponseBytes);

            Assert.NotNull(actualHostname);
            Assert.Equal(hostname, actualHostname.Value<string>());

            Assert.NotNull(actualForwardedHostname);
            Assert.Equal(forwardedHostname, actualForwardedHostname.Value<string>());

            Assert.NotNull(actualIp);
            Assert.Equal(ip, actualIp.Value<string>());
        }

        [Fact(DisplayName = "Log HTTP Post Request info is not null")]
        public void Formatter_Http_Post_Request_Context_Info_Is_Not_Null()
        {
            var correlationId = Guid.NewGuid().ToString();
            var hasHttpRequest = true;
            var url = "/api/v1/customers";
            var method = "POST";
            var body = "My Body Request";
            var contentType = "application/json";
            var scheme = "http";
            var protocol = "HTTP/1.1";
            var userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            var hostname = "localhost";
            var forwardedHostname = "id42.example-cdn.com";
            var ip = "127.0.0.1";
            var hasHttpResponse = true;
            var statusCode = 200;
            var responseTime = 1500000;
            var responseBytes = 10;

            var json = AssertValidJson(log => log
                .ForContext("CorrelationId", correlationId)
                .ForContext("HasHttpRequest", hasHttpRequest)
                .ForContext("Url", url)
                .ForContext("Method", method)
                .ForContext("RequestBody", body)
                .ForContext("ContentType", contentType)
                .ForContext("Scheme", scheme)
                .ForContext("Protocol", protocol)
                .ForContext("UserAgent", userAgent)
                .ForContext("HasHttpResponse", hasHttpResponse)
                .ForContext("StatusCode", statusCode)
                .ForContext("ResponseTime", responseTime)
                .ForContext("ResponseBytes", responseBytes)
                .ForContext("Hostname", hostname)
                .ForContext("ForwardedHostname", forwardedHostname)
                .ForContext("Ip", ip)
                .Information("Incoming Request"));

            var actualCorrelationId = json["CorrelationId"];
            var actualUrl = json["Http"]["Request"]["Path"];
            var actualMethod = json["Http"]["Request"]["Method"];
            var actualBody = json["Http"]["Request"]["Body"];
            var actualContentType = json["Http"]["Request"]["ContentType"];
            var actualScheme = json["Http"]["Request"]["Scheme"];
            var actualProtocol = json["Http"]["Request"]["Protocol"];
            var actualUserAgent = json["Http"]["Request"]["UserAgent"];
            var actualStatusCode = json["Http"]["Response"]["StatusCode"];
            var actualResponseTime = json["Http"]["Response"]["ResponseTime"];
            var actualResponseBytes = json["Http"]["Response"]["Bytes"];
            var actualHostname = json["Host"]["Hostname"];
            var actualForwardedHostname = json["Host"]["ForwardedHostname"];
            var actualIp = json["Host"]["Ip"];

            Assert.NotNull(actualCorrelationId);
            Assert.Equal(correlationId, actualCorrelationId.Value<string>());

            Assert.NotNull(actualUrl);
            Assert.Equal(url, actualUrl.Value<string>());

            Assert.NotNull(actualMethod);
            Assert.Equal(method, actualMethod.Value<string>());

            Assert.NotNull(actualBody);
            Assert.Equal(actualBody, actualBody.Value<string>());

            Assert.NotNull(actualContentType);
            Assert.Equal(contentType, actualContentType.Value<string>());

            Assert.NotNull(actualScheme);
            Assert.Equal(scheme, actualScheme.Value<string>());

            Assert.NotNull(actualProtocol);
            Assert.Equal(protocol, actualProtocol.Value<string>());

            Assert.NotNull(actualUserAgent);
            Assert.Equal(userAgent, actualUserAgent.Value<string>());

            Assert.NotNull(actualStatusCode);
            Assert.Equal(statusCode, actualStatusCode);

            Assert.NotNull(actualResponseTime);
            Assert.Equal(responseTime, actualResponseTime);

            Assert.NotNull(actualResponseBytes);
            Assert.Equal(responseBytes, actualResponseBytes);

            Assert.NotNull(actualHostname);
            Assert.Equal(hostname, actualHostname.Value<string>());

            Assert.NotNull(actualForwardedHostname);
            Assert.Equal(forwardedHostname, actualForwardedHostname.Value<string>());

            Assert.NotNull(actualIp);
            Assert.Equal(ip, actualIp.Value<string>());
        }
    
    }
}
