using System;
using System.Net.Http;
using DisTrace.Core;
using DisTrace.TestBase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace DisTrace.AspNetCore.Tests
{
    public class AspNetCoreServerTest : ServerTest
    {
        public override Func<ITracingContextProvider, HttpMessageHandler> CreateServerWithTracingContext =>
            serverTracingContextProvider =>
            {
                var webHostBuilder = new WebHostBuilder();
                webHostBuilder
                    .ConfigureServices(s => { s.AddScoped(_ => serverTracingContextProvider); })
                    .UseStartup<Startup>();
                var server = new TestServer(webHostBuilder);
                return server.CreateHandler();
            };
    }
}