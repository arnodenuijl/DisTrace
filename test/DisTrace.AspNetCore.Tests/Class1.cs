using System;
using System.Net.Http;
using DisTrace.Core;
using DisTrace.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace DisTrace.AspNetCore.Tests
{
    public class AspNetCoreIntegrationTest : IntegrationTest
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


    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<SetTracingContextFromRequestMiddleware>();

            app.Run(async context => { await context.Response.WriteAsync("Hello, World!"); });
        }
    }
}