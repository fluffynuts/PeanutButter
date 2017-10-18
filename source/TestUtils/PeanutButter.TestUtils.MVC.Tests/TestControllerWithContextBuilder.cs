using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Routing;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.TestUtils.MVC.Builders;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.MVC.Tests
{
    [TestFixture]
    public class TestControllerWithContextBuilder
    {
        [Test]
        public void Build_Default_ShouldBuild()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                    ControllerWithContextBuilder<SomeController>.Create().Build()
                )
                .Not.To.Throw();

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldSetUpControllerContext()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.ControllerContext).Not.To.Be.Null();
        }


        [Test]
        public void Build_GivenPrincipal_ShouldSetUpUser()
        {
            //--------------- Arrange -------------------
            var principal = Substitute.For<IPrincipal>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .WithPrincipal(principal)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.User).To.Equal(principal);
        }

        [Test]
        public void Build_GivenAction_ShouldSetUpRouteData()
        {
            //--------------- Arrange -------------------
            var action = GetRandomString(5);
            var parameter = GetRandomString(5);
            var value = GetRandomString(5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .ForAction(action)
                .WithQueryStringParameter(parameter, value)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.RouteData.Values["action"]).To.Equal(action);
            Expect(controller.RouteData.Values["controller"])
                .To.Equal(nameof(SomeController).Replace("Controller", ""));
        }

        [Test]
        public void Build_GivenAndQueryStringParameters_ShouldSetUpQueryString()
        {
            //--------------- Arrange -------------------
            var action = GetRandomString(5);
            var parameter = GetRandomString(5);
            var value = GetRandomString(5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .ForAction(action)
                .WithQueryStringParameter(parameter, value)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.Request.QueryString[parameter]).To.Equal(value);
        }

        [Test]
        public void Build_GivenRouteData_ShouldSetRouteData()
        {
            //--------------- Arrange -------------------
            var expected = new RouteData();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .WithRouteData(expected)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.RouteData).To.Equal(expected);
        }

        [Test]
        public void Build_GivenRouteValues_ShouldSetRouteDataValues()
        {
            //--------------- Arrange -------------------
            var expected = new RouteValueDictionary();
            var key = GetRandomString(5);
            var value = GetRandomString();
            expected[key] = value;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .WithRouteValues(expected)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.RouteData.Values[key]).To.Equal(expected[key]);
        }


        [Test]
        public void Build_GivenSessionItem_ShouldSetSessionItemValue()
        {
            //--------------- Arrange -------------------
            var key = GetRandomString(5);
            var value = GetRandomString(5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .WithSessionItem(key, value)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.Session[key]).To.Equal(value);
        }

        [Test]
        public void Build_GivenCookieData_ShouldSetCookie()
        {
            //--------------- Arrange -------------------
            var key = GetRandomString(5);
            var value = GetRandomString(5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .WithCookie(key, value)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.Request.Cookies[key]?.Value).To.Equal(value);
        }


        [Test]
        public void Build_GivenFormParameter_ShouldSetFormParameter()
        {
            //--------------- Arrange -------------------
            var key = GetRandomString(5);
            var value = GetRandomString(5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .WithFormParameter(key, value)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.Request.Form[key]).To.Equal(value);
        }

        [Test]
        public void Build_GivenFormParametersCollection_ShouldSetFormParameters()
        {
            //--------------- Arrange -------------------
            var expected = new NameValueCollection();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .WithFormParameters(expected)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller.Request.Form == expected).To.Be.True();
        }

        [Test]
        public void Build_GivenControllerFactory_ShouldUseItToInstantiateController()
        {
            //--------------- Arrange -------------------
            var expected = new SomeController();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var controller = ControllerWithContextBuilder<SomeController>
                .Create()
                .WithControllerFactory(() => expected)
                .Build();

            //--------------- Assert -----------------------
            Expect(controller).To.Equal(expected);
        }

        [Test]
        public void Build_WhenNoControllerFactoryAndControllerHasConstructorParameters_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => ControllerWithContextBuilder<HasConstructorArgumentsController>
                    .Create()
                    .Build()
               ).To.Throw().With.Message.Containing("no parameterless constructor");

            //--------------- Assert -----------------------
        }
    }

    public class SomeController : Controller
    {
    }

    public class HasConstructorArgumentsController : Controller
    {
        public bool IsContrived { get; }

        public HasConstructorArgumentsController(bool isContrived)
        {
            IsContrived = isContrived;
        }
    }
}