using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <summary>
    /// Actually rendering a view is going to be tricky, and probably
    /// unnecessary. For testing purposes, the caller needs to be able
    /// to assert the view name and the provided data
    /// </summary>
    internal class FakeViewResultExecutor
        : IActionResultExecutor<PartialViewResult>,
          IActionResultExecutor<ViewResult>
    {
        public async Task ExecuteAsync(
            ActionContext context,
            PartialViewResult result
        )
        {
            await Resolve(
                context,
                result.StatusCode,
                result.ContentType,
                result.Model,
                result.ViewName,
                result.ViewData
            );
        }

        private async Task Resolve(
            ActionContext context,
            int? statusCode,
            string contentType,
            object model,
            string viewName,
            ViewDataDictionary viewData
        )
        {
            await context.HttpContext.Response.WriteAsync($"[{viewName}]");
            context.HttpContext.Items["Model"] = model;
            context.HttpContext.Items["ViewData"] = viewData;
            context.HttpContext.Response.StatusCode =
                statusCode ?? (int) HttpStatusCode.OK;
            context.HttpContext.Response.ContentType =
                contentType ?? "text/html";
        }

        public async Task ExecuteAsync(
            ActionContext context,
            ViewResult result
        )
        {
            await Resolve(
                context,
                result.StatusCode,
                result.ContentType,
                result.Model,
                result.ViewName,
                result.ViewData
            );
        }
    }
}