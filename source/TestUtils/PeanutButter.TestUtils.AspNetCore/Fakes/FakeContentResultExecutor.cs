using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

internal class FakeContentResultExecutor : IActionResultExecutor<ContentResult>
{
    public async Task ExecuteAsync(
        ActionContext context,
        ContentResult result
    )
    {
        await context.HttpContext.Response.WriteAsync(
            result.Content ?? ""
        );
        context.HttpContext.Response.StatusCode =
            result.StatusCode ?? (int)HttpStatusCode.OK;
        context.HttpContext.Response.ContentType =
            result.ContentType ?? "text/html";
    }
}