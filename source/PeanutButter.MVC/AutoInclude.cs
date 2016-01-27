using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace PeanutButter.MVC
{
    public static class AutoInclude
    {
        public static string BundleBase { get; set; } = "~/bundles/js/";

        public static IHtmlString AutoIncludeScriptsFor(ViewContext viewContext, IBundleResolver bundleResolver = null, Func<string[], IHtmlString> scriptRenderer = null)
        {
            bundleResolver = bundleResolver ?? BundleResolver.Current;
            scriptRenderer = scriptRenderer ?? Scripts.Render;

            return new HtmlString(string.Join("\n", GetControllerScripts(viewContext, bundleResolver, scriptRenderer)
                                                    .Union(GetActionScripts(viewContext, bundleResolver, scriptRenderer))));
        }

        private static IEnumerable<string> GetActionScripts(ViewContext viewContext, IBundleResolver bundleResolver, Func<string[], IHtmlString> scriptRenderer)
        {
            var scripts = new List<IHtmlString>();
            var actionScripts = GetActionScriptsBundleNameFor(viewContext);
            var bundle = bundleResolver.GetBundleContents(actionScripts);
            if (bundle != null && bundle.Any())
            {
                scripts.Add(scriptRenderer(new[] {actionScripts}));
            }
            return scripts.Select(s => s.ToHtmlString());
        }

        private static IEnumerable<string> GetControllerScripts(ViewContext viewContext, IBundleResolver bundleResolver, Func<string[], IHtmlString> scriptRenderer)
        {
            var scripts = new List<IHtmlString>();
            var controllerScripts = GetControllerScriptsBundleNameFor(viewContext);
            var bundle = bundleResolver.GetBundleContents(controllerScripts);
            if (bundle != null && bundle.Any())
            {
                scripts.Add(scriptRenderer(new[] {controllerScripts}));
            }
            return scripts.Select(s => s.ToHtmlString());
        }

        private static string GetControllerScriptsBundleNameFor(ViewContext viewContext)
        {
            var controller = GetControllerNameFor(viewContext);
            return BundleBase + controller.ToLower();
        }

        private static string GetActionScriptsBundleNameFor(ViewContext viewContext)
        {
            var controller = GetControllerNameFor(viewContext);
            var action = GetActionNameFor(viewContext);
            return string.Join(string.Empty, BundleBase, controller, "/", action);
        }

        private static string GetControllerNameFor(ViewContext viewContext)
        {
            return GetContextValue(viewContext, "Controller");
        }

        private static string GetActionNameFor(ViewContext viewContext)
        {
            return GetContextValue(viewContext, "Action");
        }

        private static string GetContextValue(ViewContext viewContext, string name)
        {
            var valueProvider = viewContext.Controller.ValueProvider;
            var value = valueProvider.GetValue(name).RawValue.ToString();
            return value.ToLower();
        }
    }
}
