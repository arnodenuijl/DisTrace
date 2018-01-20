using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DisTrace.HttpClient.Tests
{
    class DummyHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        public HttpRequestMessage Request { get; set; }
    }
}