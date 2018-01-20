using System;
using System.Linq;
using System.Threading.Tasks;
using DisTrace.Core;
using Microsoft.AspNetCore.Http;

namespace DisTrace.AspNetCore
{
    public class SetTracingContextFromRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public SetTracingContextFromRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ITracingContextProvider tracingContextProvider)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tracingContextProvider == null) throw new ArgumentNullException(nameof(tracingContextProvider));

            var unitOfWorkId = GetHeaderValueOrDefault(context.Request, TracingContextHeaders.RequestIdHeaderName);
            var causationId = GetHeaderValueOrDefault(context.Request, TracingContextHeaders.CausationIdHeaderName);
            var flowId = GetHeaderValueOrDefault(context.Request, TracingContextHeaders.FlowIdHeaderName);

            tracingContextProvider.SetTracingContext(new TracingContext(unitOfWorkId, causationId, flowId));

            await _next.Invoke(context);
        }

        private static string GetHeaderValueOrDefault(HttpRequest request, string headerName)
        {
            return request.Headers.TryGetValue(headerName, out var headers)
                ? headers.LastOrDefault(s => !string.IsNullOrWhiteSpace(s))
                : null;
        }
    }
}