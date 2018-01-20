using System.Linq;
using System.Net;
using System.Net.Http;
using DisTrace.Core;
using Xunit;

namespace DisTrace.HttpClient.Tests
{
    public class HttpClientTest
    {
        [Fact]
        public void tracingContext_with_uow_and_flowId_sets_all()
        {
            var singleTracingContextProvider = new SingleTracingContextProvider();
            var httpMessageHandler = new DummyHandler();
            var addTracingContextToRequestHandler = new AddTracingContextToRequestHandler(singleTracingContextProvider, httpMessageHandler);
            var client = new System.Net.Http.HttpClient(addTracingContextToRequestHandler);

            var tracingContext = TracingContext.CreateFromUnitOfWorkId("req1");

            singleTracingContextProvider.SetTracingContext(tracingContext);
            var result = client.GetAsync("http://niks.nl");

            var requestId = GetHeaderValueOrDefault(httpMessageHandler.Request, TracingContextHeaders.RequestIdHeaderName);
            var causationId = GetHeaderValueOrDefault(httpMessageHandler.Request, TracingContextHeaders.CausationIdHeaderName);
            var flowId = GetHeaderValueOrDefault(httpMessageHandler.Request, TracingContextHeaders.FlowIdHeaderName);

            Assert.Equal(HttpStatusCode.OK, result.Result.StatusCode);
            Assert.StartsWith("req1.", requestId);
            Assert.Equal("req1", causationId);
            Assert.Equal("req1", flowId);
        }

        [Fact]
        public void no_tracingContext_only_sets_requestId()
        {
            var singleTracingContextProvider = new SingleTracingContextProvider();
            var httpMessageHandler = new DummyHandler();
            var addTracingContextToRequestHandler = new AddTracingContextToRequestHandler(singleTracingContextProvider, httpMessageHandler);
            var client = new System.Net.Http.HttpClient(addTracingContextToRequestHandler);

            var result = client.GetAsync("http://niks.nl");

            var requestId = GetHeaderValueOrDefault(httpMessageHandler.Request, TracingContextHeaders.RequestIdHeaderName);
            var causationId = GetHeaderValueOrDefault(httpMessageHandler.Request, TracingContextHeaders.CausationIdHeaderName);
            var flowId = GetHeaderValueOrDefault(httpMessageHandler.Request, TracingContextHeaders.FlowIdHeaderName);

            Assert.Equal(HttpStatusCode.OK, result.Result.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(requestId));
            Assert.Null(causationId);
            Assert.Null(flowId);
        }

        private static string GetHeaderValueOrDefault(HttpRequestMessage request, string headerName)
        {
            return request.Headers.TryGetValues(headerName, out var headers)
                ? headers.LastOrDefault(s => !string.IsNullOrWhiteSpace(s))
                : null;
        }

    }
}