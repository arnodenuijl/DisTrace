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
            if (request == null) throw new ArgumentNullException(nameof(request));

            var unitOfWorkId = GetHeaderValueOrDefault(request, TracingContextHeaders.RequestIdHeaderName);
            var causationId = GetHeaderValueOrDefault(request, TracingContextHeaders.CausationIdHeaderName);
            var flowId = GetHeaderValueOrDefault(request, TracingContextHeaders.FlowIdHeaderName);

            _tracingContextProvider.SetTracingContext(new TracingContext(unitOfWorkId, causationId, flowId));
            return base.SendAsync(request, cancellationToken);
        }

        private static string GetHeaderValueOrDefault(HttpRequestMessage request, string headerName)
        {
            return request.Headers.TryGetValues(headerName, out var headers)
                ? headers.LastOrDefault(s => !string.IsNullOrWhiteSpace(s))
                : null;
        }
    }
}