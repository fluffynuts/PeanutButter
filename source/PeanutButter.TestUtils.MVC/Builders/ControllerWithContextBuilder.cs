using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.AspNet.Identity;
using PeanutButter.TestUtils.MVC.Builders;
using NSubstitute;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class ControllerWithContextBuilder<T> where T: ControllerBase
    {
        private string _userName;
        private Guid _userGuid;
        private List<string> _roles;
        private NameValueCollection _formParameters;
        private NameValueCollection _queryStringParameters;
        private HttpCookieCollection _cookies;
        private SessionStateItemCollection _sessionState;
        private RouteData _routeData;
        private Func<T> _controllerFactoryFunc;
        private IPrincipal _principal;

        public static ControllerWithContextBuilder<T> Create()
        {
            return new ControllerWithContextBuilder<T>();
        }

        public ControllerWithContextBuilder()
        {
            WithUserName("Anonymous")
                .WithUserGuid(Guid.NewGuid())
                .WithRoles()
                .WithFormParameters(new NameValueCollection())
                .WithQueryStringParameters(new NameValueCollection())
                .WithCookies(new HttpCookieCollection())
                .WithSessionStateItems(new SessionStateItemCollection())
                .WithRouteData(new RouteData()
                {
                    Route = new Route("{controller}/{action}/{id}", Substitute.For<IRouteHandler>())
                });
            SetControllerRouteDataFromType();
        }

        public T Build()
        {
            var controller = SpawnControllerInstance();
            var httpContext = new FakeHttpContext(
                _principal ?? CreatePrincipal(),
                _formParameters,
                _queryStringParameters,
                _cookies,
                _sessionState);
            controller.ControllerContext = new ControllerContext(httpContext, _routeData, controller);
            return controller;
        }

        public ControllerWithContextBuilder<T> WithPrincipal(IPrincipal principal)
        {
            _principal = principal;
            return this;
        }

        private ClaimsPrincipal CreatePrincipal()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, _userName));
            claims.Add(new Claim(ExtendedClaimTypes.IdentityProvider, _userGuid.ToString()));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, _userGuid.ToString()));
            claims.AddRange(_roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var claimsIdentity = new ClaimsIdentity(claims, 
                DefaultAuthenticationTypes.ApplicationCookie,
                ClaimTypes.Name,
                ClaimTypes.Role);
            return new ClaimsPrincipal(claimsIdentity);
        }

        private T SpawnControllerInstance()
        {
            T controller = null;
            if (_controllerFactoryFunc == null)
            {
                try
                {
                    controller = Activator.CreateInstance<T>();
                }
                catch
                {
                    throw new Exception($"Controller builder function not defined to generate controller of type '{typeof(T).Name}' and no default constructor available");
                }
            }
            else
            {
                controller = _controllerFactoryFunc();
                if (controller == null)
                    throw new Exception("Provided controller factory function returns null. Tsk.");
            }
            return controller;
        }

        public ControllerWithContextBuilder<T> WithControllerFactory(Func<T> controllerFactoryFunc)
        {
            _controllerFactoryFunc = controllerFactoryFunc;
            return this;
        }

        public ControllerWithContextBuilder<T> WithRouteData(RouteData routeData)
        {
            _routeData = routeData;
            return this;
        }

        public ControllerWithContextBuilder<T> WithRouteValues(RouteValueDictionary routeValues)
        {
            _routeData.Values.Clear();
            foreach (var kvp in routeValues)
                _routeData.Values.Add(kvp.Key, kvp.Value);
            SetControllerRouteDataFromType();
            return this;
        }

        private void SetControllerRouteDataFromType()
        {
            var regex = new Regex("Controller$");
            _routeData.Values["controller"] = regex.Replace(typeof (T).Name, string.Empty);
        }

        public ControllerWithContextBuilder<T> ForAction(string action)
        {
            _routeData.Values["action"] = action;
            return this;
        }

        public ControllerWithContextBuilder<T> WithSessionStateItems(SessionStateItemCollection collection)
        {
            _sessionState = collection;
            return this;
        }

        public ControllerWithContextBuilder<T> WithSessionItem(string name, object value)
        {
            _sessionState[name] = value;
            return this;
        }

        public ControllerWithContextBuilder<T> WithUserName(string name)
        {
            _userName = name;
            return this;
        }

        public ControllerWithContextBuilder<T> WithUserGuid(Guid id)
        {
            _userGuid = id;
            return this;
        }

        public ControllerWithContextBuilder<T> WithRoles(params string[] roles)
        {
            if (_roles == null)
                _roles = new List<string>();
            _roles.AddRange(roles);
            return this;
        }

        public ControllerWithContextBuilder<T> WithFormParameters(NameValueCollection collection)
        {
            _formParameters = collection;
            return this;
        }

        public ControllerWithContextBuilder<T> WithFormParameter<TValue>(string name, TValue value)
        {
            var valueAsText = GetTextFor(value);
            _formParameters.Add(name, valueAsText);
            return this;
        }

        private string GetTextFor(object value)
        {
            var asBytes = value as byte[];
            if (asBytes != null)
                return Convert.ToBase64String(asBytes);
            return value.ToString();
        }

        public ControllerWithContextBuilder<T> WithQueryStringParameters(NameValueCollection collection)
        {
            _queryStringParameters = collection;
            return this;
        }

        public ControllerWithContextBuilder<T> WithQueryStringParameter(string name, string value)
        {
            _queryStringParameters.Add(name, value);
            return this;
        }

        public ControllerWithContextBuilder<T> WithCookies(HttpCookieCollection collection)
        {
            _cookies = collection;
            return this;
        }

        public ControllerWithContextBuilder<T> WithCookie(string name, string value, DateTime? expires = null)
        {
            var cookie = new HttpCookie(name, value);
            if (expires.HasValue)
                cookie.Expires = expires.Value;
            _cookies.Add(cookie);
            return this;
        }

    }

    public static class ExtendedClaimTypes
    {
        public const string IdentityProvider =
            "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
    }
}