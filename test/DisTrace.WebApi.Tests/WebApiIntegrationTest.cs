using System;
using System.Net.Http;
using System.Web.Http;
using DisTrace.Core;
using DisTrace.TestBase;

namespace DisTrace.WebApi.Tests
{
    public class WebApiIntegrationTest : IntegrationTest
    {
        public override Func<ITracingContextProvider, HttpMessageHandler> CreateServerWithTracingContext =>
            serverTracingContextProvider =>
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute("Default", "api/{controller}/{action}/{id}",
                    new {id = RouteParameter.Optional});
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                config.MessageHandlers.Add(new SetTracingContextFromRequestHandler(serverTracingContextProvider));
                return new HttpServer(config);
            };
    }
}