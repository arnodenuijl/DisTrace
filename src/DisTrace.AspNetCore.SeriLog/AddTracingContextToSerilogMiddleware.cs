using System;
using System.Threading.Tasks;
using DisTrace.Core;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace DisTrace.AspNetCore.SeriLog
{
    /// <summary>
    ///     ASP.Net core middleware
    ///     Can be used to get the tracingcontext properties in the logging
    /// </summary>
    public class AddTracingContextToSerilogMiddleware
    {
        private readonly RequestDelegate _next;

        public AddTracingContextToSerilogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        ///     Asks the service container for the tracingcontext for the current call
        ///     and adds it to the logcontext for this call
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tracingContextProvider"></param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task Invoke(HttpContext context, ITracingContextProvider tracingContextProvider)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tracingContextProvider == null) throw new ArgumentNullException(nameof(tracingContextProvider));

            if (tracingContextProvider.GetTracingContext() is TracingContext tracingContext)
                using (LogContext.PushProperty("TracingContext", tracingContext, true))
                {
                    await _next(context);
                }
            else
                await _next(context);
        }
    }
}