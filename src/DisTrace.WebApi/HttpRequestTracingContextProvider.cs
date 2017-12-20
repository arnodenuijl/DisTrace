using System.Web;
using DisTrace.Core;

namespace DisTrace.WebApi
{
    public class HttpRequestTracingContextProvider : ITracingContextProvider
    {
        public TracingContext GetTracingContext()
        {
            return HttpContext.Current.Items["TracingContext"] as TracingContext;
        }

        public void SetTracingContext(TracingContext tracingContext)
        {
            HttpContext.Current.Items["TracingContext"] = tracingContext;
        }
    }
}