using System;
using System.Net.Http;
using System.Threading.Tasks;
using DisTrace.Core;
using DisTrace.HttpClient;
using Xunit;

namespace DisTrace.TestBase
{
    public abstract class IntegrationTest
    {
        public abstract Func<ITracingContextProvider, HttpMessageHandler> CreateServerWithTracingContext { get; }


        public async Task<TracingContext> Test(TracingContext clientTracingContext)
        {
            var serverTracingContextProvider = new SingleTracingContextProvider();

            var clientTracingContextProvider = new SingleTracingContextProvider();
            clientTracingContextProvider.SetTracingContext(clientTracingContext);

            var serverWithTracingContext = CreateServerWithTracingContext(serverTracingContextProvider);

            var httpClient = new System.Net.Http.HttpClient(
                new AddTracingContextToRequestHandler(clientTracingContextProvider, serverWithTracingContext));

            await httpClient.GetAsync(new Uri("http://niks.nl/api/Home/Index"));

            return serverTracingContextProvider.GetTracingContext();
        }

        [Fact]
        public async Task empty_tracing_context()
        {
            var x = await Test(new TracingContext(null, null, null));
            Assert.True(
                !string.IsNullOrWhiteSpace(x.RequestId) &&
                x.CausationId == null && x.CorrelationId == x.RequestId);
        }

        [Fact]
        public async Task null_tracing_context()
        {
            var x = await Test(null);
            Assert.True(
                !string.IsNullOrWhiteSpace(x.RequestId) &&
                x.CausationId == null && x.CorrelationId == x.RequestId);
        }

        [Fact]
        public async Task only_requestId()
        {
            var x = await Test(new TracingContext("req1", null, null));
            Assert.True(
                !string.IsNullOrWhiteSpace(x.RequestId) &&
                x.CausationId == "req1" && x.CorrelationId == "req1");
        }

        [Fact]
        public async Task requestId_and_correlationId()
        {
            var x = await Test(new TracingContext("req1", null, "cor1"));
            Assert.True(
                !string.IsNullOrWhiteSpace(x.RequestId) &&
                x.CausationId == "req1" && x.CorrelationId == "cor1");
        }
    }
}