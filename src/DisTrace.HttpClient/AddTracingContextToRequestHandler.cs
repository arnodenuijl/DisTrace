using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DisTrace.Core;

namespace DisTrace.HttpClient
{
    public class AddTracingContextToRequestHandler : DelegatingHandler
    {
        private static readonly Random Random = new Random();
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
                var requestId = context.UnitOfWorkId + "." + GenerateRandomString(8);
                request.Headers.Add(TracingContextHeaders.RequestIdHeaderName, requestId);
                request.Headers.Add(TracingContextHeaders.CausationIdHeaderName, context.UnitOfWorkId);
                request.Headers.Add(TracingContextHeaders.FlowIdHeaderName, context.FlowId);
            }
            else
            {
                request.Headers.Add(TracingContextHeaders.RequestIdHeaderName, GenerateRandomString(8));
            }

            return base.SendAsync(request, cancellationToken);
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}