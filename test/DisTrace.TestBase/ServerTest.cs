using System;
using System.Net.Http;
using System.Threading.Tasks;
using DisTrace.Core;
using Xunit;

namespace DisTrace.TestBase
{
    public abstract class ServerTest
    {
        public abstract Func<ITracingContextProvider, HttpMessageHandler> CreateServerWithTracingContext { get; }

        public async Task<TracingContext> TestServerContext(string requestId, string causationId, string flowId)
        {
            var serverTracingContextProvider = new SingleTracingContextProvider();

            var serverWithTracingContext = CreateServerWithTracingContext(serverTracingContextProvider);
            var httpClient = new System.Net.Http.HttpClient(serverWithTracingContext);
            if (requestId != null)
                httpClient.DefaultRequestHeaders.Add(TracingContextHeaders.RequestIdHeaderName, new[] {requestId});

            if (flowId != null)
                httpClient.DefaultRequestHeaders.Add(TracingContextHeaders.FlowIdHeaderName, new[] {flowId});

            if (causationId != null)
                httpClient.DefaultRequestHeaders.Add(TracingContextHeaders.CausationIdHeaderName, new[] {causationId});

            await httpClient.GetAsync(new Uri("http://niks.nl/api/Home/Index"));
            return serverTracingContextProvider.GetTracingContext();
        }

        [Fact]
        public async Task empty_tracing_context()
        {
            var x = await TestServerContext("", "", "");
            Assert.False(string.IsNullOrWhiteSpace(x.UnitOfWorkId),
                "Server should generate unitOfWorkId if not provided");
            Assert.True(x.CausationId == null, "causationId should be empty when no requestId was send");
            Assert.Equal(x.FlowId, x.UnitOfWorkId);
        }

        [Fact]
        public async Task null_tracing_context()
        {
            var x = await TestServerContext(null, null, null);
            Assert.False(string.IsNullOrWhiteSpace(x.UnitOfWorkId),
                "Server should generate unitOfWorkId if not provided");
            Assert.True(x.CausationId == null, "causationId should be empty when no causationId was send");
            Assert.Equal(x.FlowId, x.UnitOfWorkId);
        }

        [Fact]
        public async Task only_requestId_sets_uow_and_flow()
        {
            var serverTracingContext = await TestServerContext("req1", null, null);
            Assert.Equal("req1", serverTracingContext.UnitOfWorkId);
            Assert.Null(serverTracingContext.CausationId);
            Assert.Equal("req1", serverTracingContext.FlowId);
        }

        [Fact]
        public async Task only_causationId_sets_it_and_generates_uow_and_sets_flowId_to_causationId()
        {
            var serverTracingContext = await TestServerContext(null, "caus", null);
            Assert.False(string.IsNullOrWhiteSpace(serverTracingContext.UnitOfWorkId),
                "Server should generate unitOfWorkId if not provided");
            Assert.Equal("caus", serverTracingContext.CausationId);
            Assert.Equal("caus", serverTracingContext.FlowId);
        }

        [Fact]
        public async Task only_flowId_sets_it_and_generates_uowId()
        {
            var serverTracingContext = await TestServerContext(null, null, "flow1");
            Assert.False(string.IsNullOrWhiteSpace(serverTracingContext.UnitOfWorkId),
                "Server should generate unitOfWorkId if not provided");
            Assert.Null(serverTracingContext.CausationId);
            Assert.Equal("flow1", serverTracingContext.FlowId);
        }

        [Fact]
        public async Task requestId_and_flowId()
        {
            var serverTracingContext = await TestServerContext("req1", null, "flow1");
            Assert.Equal("req1", serverTracingContext.UnitOfWorkId);
            Assert.Null(serverTracingContext.CausationId);
            Assert.Equal("flow1", serverTracingContext.FlowId);
        }
    }
}