using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DisTrace.Core;
using Serilog.Context;

namespace DisTrace.WebApi.SeriLog
{
    public class AddTracingContextToSerilogHandler : DelegatingHandler
    {
        private readonly ITracingContextProvider _tracingContextProvider;

        public AddTracingContextToSerilogHandler(ITracingContextProvider tracingContextProvider)
        {
            _tracingContextProvider = tracingContextProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var tracingContext = _tracingContextProvider.GetTracingContext();
            using (LogContext.PushProperty("TracingContext", tracingContext, true))
            {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}