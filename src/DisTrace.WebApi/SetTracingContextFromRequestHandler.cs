using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DisTrace.Core;

namespace DisTrace.WebApi
{
    public class SetTracingContextFromRequestHandler : DelegatingHandler
    {
        private readonly ITracingContextProvider _tracingContextProvider;

        public SetTracingContextFromRequestHandler(ITracingContextProvider tracingContextProvider)
        {
            _tracingContextProvider = tracingContextProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid().ToString();
            var causationId = GetRequestIdFromRequestOrNull(request);
            var correlationId = GetCorrelationIdFromRequestOrNull(request) ?? causationId ?? requestId;

            _tracingContextProvider.SetTracingContext(new TracingContext(requestId, causationId, correlationId));

            return base.SendAsync(request, cancellationToken);
        }

        private static string GetRequestIdFromRequestOrNull(HttpRequestMessage requestMessage)
        {
            return requestMessage.Headers.TryGetValues(TracingContextHeaders.RequestIdHeaderName, out var headers)
                ? headers.LastOrDefault(s => !string.IsNullOrWhiteSpace(s))
                : null;
        }

        private static string GetCorrelationIdFromRequestOrNull(HttpRequestMessage requestMessage)
        {
            return requestMessage.Headers.TryGetValues(TracingContextHeaders.CorrelationIdHeaderName, out var headers)
                ? headers.LastOrDefault(s => !string.IsNullOrWhiteSpace(s))
                : null;
        }
    }
}