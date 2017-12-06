using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.MVC.Tests
{
    [TestFixture]
    public class TestAutoInclude
    {
        [Test]
        public void AutoIncludeScriptsFor_GivenViewContextAndBundleResolverWithNoMatchingBundles_ReturnsEmptyHTMLString()
        {
            //---------------Set up test pack-------------------
            var ctx = new ViewContext {Controller = Substitute.For<Controller>()};
            var valueProvider = Substitute.For<IValueProvider>();
            var controllerName = RandomValueGen.GetRandomString();
            var actionName = RandomValueGen.GetRandomString();
            valueProvider.GetValue("Controller").Returns(new ValueProviderResult(controllerName, controllerName, CultureInfo.InvariantCulture));
            valueProvider.GetValue("Action").Returns(new ValueProviderResult(actionName, actionName, CultureInfo.InvariantCulture));

            ctx.Controller.ValueProvider = valueProvider;

            var bundleResolver = Substitute.For<IBundleResolver>();

            //---------------Assert Precondition----------------
            Assert.AreEqual(0, bundleResolver.GetBundleContents(AutoInclude.BundleBase + controllerName).Count());

            //---------------Execute Test ----------------------
            var result = AutoInclude.AutoIncludeScriptsFor(ctx, bundleResolver);

            //---------------Test Result -----------------------
            Assert.AreEqual("", result.ToHtmlString());
        }

        [Test]
        public void AutoIncludeScriptsFor_GivenViewContextAndBundleResolverWithControllerScript_ReturnsStringForControllerScript()
        {
            //---------------Set up test pack-------------------
            var ctx = new ViewContext {Controller = Substitute.For<Controller>()};
            var valueProvider = Substitute.For<IValueProvider>();
            var controllerName = RandomValueGen.GetRandomString();
            var actionName = RandomValueGen.GetRandomString();
            valueProvider.GetValue("Controller").Returns(new ValueProviderResult(controllerName, controllerName, CultureInfo.InvariantCulture));
            valueProvider.GetValue("Action").Returns(new ValueProviderResult(actionName, actionName, CultureInfo.InvariantCulture));

            ctx.Controller.ValueProvider = valueProvider;

            var bundleResolver = Substitute.For<IBundleResolver>();
            var script = RandomValueGen.GetRandomString() + ".js";
            bundleResolver.GetBundleContents(AutoInclude.BundleBase + controllerName).Returns(new[] { script });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = AutoInclude.AutoIncludeScriptsFor(ctx, bundleResolver, (name) => new HtmlString(
                $"<script src=\"{script}\"></script>"));

            //---------------Test Result -----------------------
            var parts = result.ToHtmlString().Split('\n');
            Assert.AreEqual(1, parts.Length);
            StringAssert.Contains(script, parts[0]);
        }

        [Test]
        public void AutoIncludeScriptsFor_GivenViewContextAndBundleResolverWithActionScript_ReturnsStringForControllerScript()
        {
            //---------------Set up test pack-------------------
            var ctx = new ViewContext {Controller = Substitute.For<Controller>()};
            var valueProvider = Substitute.For<IValueProvider>();
            var controllerName = RandomValueGen.GetRandomString();
            var actionName = RandomValueGen.GetRandomString();
            valueProvider.GetValue("Controller").Returns(new ValueProviderResult(controllerName, controllerName, CultureInfo.InvariantCulture));
            valueProvider.GetValue("Action").Returns(new ValueProviderResult(actionName, actionName, CultureInfo.InvariantCulture));

            ctx.Controller.ValueProvider = valueProvider;

            var bundleResolver = Substitute.For<IBundleResolver>();
            var script = RandomValueGen.GetRandomString() + ".js";
            bundleResolver.GetBundleContents(AutoInclude.BundleBase + controllerName + "/" + actionName).Returns(new[] { script });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = AutoInclude.AutoIncludeScriptsFor(ctx, bundleResolver, (name) => new HtmlString(
                $"<script src=\"{script}\"></script>"));

            //---------------Test Result -----------------------
            var parts = result.ToHtmlString().Split('\n');
            Assert.AreEqual(1, parts.Length);
            StringAssert.Contains(script, parts[0]);
        }

        [Test]
        public void AutoIncludeScriptsFor_GivenViewContextAndBundleResolverWithControllerAndActionScripts_ReturnsStringForControllerScript()
        {
            //---------------Set up test pack-------------------
            var ctx = new ViewContext {Controller = Substitute.For<Controller>()};
            var valueProvider = Substitute.For<IValueProvider>();
            var controllerName = RandomValueGen.GetRandomString();
            var actionName = RandomValueGen.GetRandomString();
            valueProvider.GetValue("Controller").Returns(new ValueProviderResult(controllerName, controllerName, CultureInfo.InvariantCulture));
            valueProvider.GetValue("Action").Returns(new ValueProviderResult(actionName, actionName, CultureInfo.InvariantCulture));

            ctx.Controller.ValueProvider = valueProvider;

            var bundleResolver = Substitute.For<IBundleResolver>();
            string c1 = RandomValueGen.GetRandomString() + ".js",
                    c2 = RandomValueGen.GetRandomString() + ".js",
                    a1 = RandomValueGen.GetRandomString() + ".js",
                    a2 = RandomValueGen.GetRandomString() + ".js";
            bundleResolver.GetBundleContents(AutoInclude.BundleBase + controllerName).Returns(new[] { c1, c2 });
            bundleResolver.GetBundleContents(AutoInclude.BundleBase + controllerName + "/" + actionName).Returns(new[] { a1, a2 });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = AutoInclude.AutoIncludeScriptsFor(ctx, bundleResolver, (names) =>
            {
                return new HtmlString(string.Join("\n", bundleResolver
                                                                .GetBundleContents(names[0])
                                                                .Select(script => $"<script src=\"{script}\"></script>")));
            });

            //---------------Test Result -----------------------
            var parts = result.ToHtmlString().Split('\n');
            Assert.AreEqual(4, parts.Length);
            Assert.IsTrue(new[] { c1, c2, a1, a2}.All(script => parts.Any(p => p.Contains(script))));
        }
    }
}
