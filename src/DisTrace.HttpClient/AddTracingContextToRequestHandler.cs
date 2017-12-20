using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DisTrace.Core;

namespace DisTrace.HttpClient
{
    public class AddTracingContextToRequestHandler : DelegatingHandler
    {
        private readonly ITracingContextProvider _tracingContextProvider;

        public AddTracingContextToRequestHandler(ITracingContextProvider tracingContextProvider,
            HttpMessageHandler innerHandler = null)
        {
            _tracingContextProvider = tracingContextProvider;
            InnerHandler = innerHandler ?? new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var context = _tracingContextProvider.GetTracingContext();
            if (context != null)
            {
                request.Headers.Add(TracingContextHeaders.RequestIdHeaderName, context.RequestId);
                request.Headers.Add(TracingContextHeaders.CorrelationIdHeaderName, context.CorrelationId);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}