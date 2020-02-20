using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PocketBookServer.Tests
{
    internal class TestHttpContext : HttpContext
    {
        public TestHttpContext()
        {
            TestResponse = new TestHttpResponse(this);
        }

        public override ConnectionInfo Connection => throw new NotImplementedException();
        public override IFeatureCollection Features => throw new NotImplementedException();
        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override HttpRequest Request => throw new NotImplementedException();
        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override HttpResponse Response => TestResponse;
        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TestHttpResponse TestResponse { get; }
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }

    internal class TestHttpResponse : HttpResponse
    {
        public TestHttpResponse(HttpContext context)
        {
            HttpContext = context;
        }

        public override Stream Body { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IResponseCookies Cookies => MockCookies.Object;
        public override bool HasStarted => throw new NotImplementedException();
        public override IHeaderDictionary Headers => throw new NotImplementedException();
        public override HttpContext HttpContext { get; }
        public Mock<IResponseCookies> MockCookies { get; } = new Mock<IResponseCookies>(MockBehavior.Loose);
        public override int StatusCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void Redirect(string location, bool permanent)
        {
            throw new NotImplementedException();
        }
    }
}