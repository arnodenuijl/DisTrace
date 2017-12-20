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

            var requestId = Guid.NewGuid().ToString();
            var causationId = GetRequestIdFromRequestOrDefault(context);
            var correlationId = GetCorrelationIdFromRequestOrDefault(context) ?? causationId ?? requestId;
            tracingContextProvider.SetTracingContext(new TracingContext(requestId, causationId, correlationId));
            await _next.Invoke(context);
        }

        private static string GetRequestIdFromRequestOrDefault(HttpContext context)
        {
            return context.Request
                .Headers[TracingContextHeaders.RequestIdHeaderName]
                .LastOrDefault(s => !string.IsNullOrWhiteSpace(s));
        }

        private static string GetCorrelationIdFromRequestOrDefault(HttpContext context)
        {
            return context.Request
                .Headers[TracingContextHeaders.CorrelationIdHeaderName]
                .LastOrDefault(s => !string.IsNullOrWhiteSpace(s));
        }
    }
}