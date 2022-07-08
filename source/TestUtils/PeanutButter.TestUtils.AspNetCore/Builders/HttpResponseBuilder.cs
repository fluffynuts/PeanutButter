using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public class HttpResponseBuilder : Builder<HttpResponseBuilder, HttpResponse>
    {
        public HttpResponseBuilder() 
            : base(Actualize)
        {
            WithStatusCode(HttpStatusCode.OK)
                .WithCookies(FakeResponseCookies.CreateSubstitutedIfPossible());
        }

        public HttpResponseBuilder WithHasStarted(bool hasStarted)
        {
            return With<FakeHttpResponse>(
                o => o.SetHasStarted(hasStarted)
            );
        }

        public HttpResponseBuilder WithOnStartingHandler(
            Action<Func<object, Task>, object> handler
        )
        {
            return With<FakeHttpResponse>(
                o => o.AddOnStartingHandler(handler)
            );
        }

        public HttpResponseBuilder WithOnCompletedHandler(
            Action<Func<object, Task>, object> handler
        )
        {
            return With<FakeHttpResponse>(
                o => o.AddOnCompletedHandler(handler)
            );
        }

        public HttpResponseBuilder WithRedirectHandler(
            Action<string, bool> handler)
        {
            return With<FakeHttpResponse>(
                o => o.AddRedirectHandler(handler)
            );
        }

        public HttpResponseBuilder WithCookies(IResponseCookies cookies)
        {
            return With<FakeHttpResponse>(
                o => o.SetCookies(cookies)
            );
        }

        public HttpResponseBuilder WithStatusCode(HttpStatusCode code)
        {
            return WithStatusCode((int) code);
        }

        public HttpResponseBuilder WithStatusCode(int code)
        {
            return With(
                o => o.StatusCode = code
            );
        }

        private static void Actualize(HttpResponse o)
        {
            WarnIf(o.HttpContext is null, "o.HttpContext is null");
            WarnIf(o.HttpContext?.Response is null, "o.HttpContext.Response is null");
        }

        protected override HttpResponse ConstructEntity()
        {
            return new FakeHttpResponse();
        }

        public override HttpResponseBuilder Randomize()
        {
            throw new NotImplementedException();
        }

        public HttpResponseBuilder WithHttpContext(HttpContext context)
        {
            return WithHttpContext(() => context);
        }

        public HttpResponseBuilder WithHttpContext(
            Func<HttpContext> accessor
        )
        {
            return With<FakeHttpResponse>(
                o => o.SetContextAccessor(accessor),
                nameof(FakeHttpResponse.HttpContext)
            );
        }
    }
}