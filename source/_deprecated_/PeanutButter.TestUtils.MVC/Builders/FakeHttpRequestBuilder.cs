using System.Collections.Specialized;
using System.Web;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class FakeHttpRequestBuilder 
        : GenericBuilderWithFieldAccess<FakeHttpRequestBuilder, FakeHttpRequest>
    {
        private NameValueCollection _headers;
        private NameValueCollection _formParameters;
        private NameValueCollection _queryStringParameters;
        private HttpCookieCollection _cookies;
        private string _url;

        public FakeHttpRequestBuilder()
        {
            WithUrl(RandomValueGen.GetRandomHttpUrl())
                .WithCookies(new HttpCookieCollection())
                .WithFormParameters(new NameValueCollection())
                .WithQueryStringParameters(new NameValueCollection())
                .WithHeaders(new NameValueCollection());
        }

        private FakeHttpRequestBuilder WithHeaders(NameValueCollection headers)
        {
            return WithField(b => b._headers = headers);
        }

        public override FakeHttpRequest ConstructEntity()
        {
            return new FakeHttpRequest(
                _formParameters,
                _queryStringParameters,
                _cookies,
                _headers,
                _url
            );
        }

        public FakeHttpRequestBuilder WithFormParameters(
            NameValueCollection formParameters
        )
        {
            return WithField(o => o._formParameters = formParameters);
        }

        public FakeHttpRequestBuilder WithQueryStringParameters(
            NameValueCollection queryStringParameters
        )
        {
            return WithField(o => o._queryStringParameters = queryStringParameters);
        }

        public FakeHttpRequestBuilder WithCookies(
            HttpCookieCollection cookies
        )
        {
            return WithField(o => o._cookies = cookies);
        }

        public FakeHttpRequestBuilder WithUrl(string url)
        {
            return WithField(o => o._url = url);
        }

    }
}
