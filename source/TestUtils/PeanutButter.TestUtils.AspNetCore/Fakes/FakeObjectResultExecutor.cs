using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    internal class FakeObjectResultExecutor : IActionResultExecutor<ObjectResult>
    {
        public async Task ExecuteAsync(
            ActionContext context,
            ObjectResult result
        )
        {
            var stringContent = SerializeToString(result);
            await context.HttpContext.Response.WriteAsync(stringContent);
            context.HttpContext.Response.StatusCode =
                result.StatusCode ?? (int) HttpStatusCode.OK;
            context.HttpContext.Response.ContentType =
                ResolveContentTypeFor(result);
        }

        private static string ResolveContentTypeFor(
            ObjectResult result
        )
        {
            return result.ContentTypes.FirstOrDefault()
                ?? "application/json";
        }

        private static string SerializeToString(
            ObjectResult result
        )
        {
            if (result.Value is null)
            {
                return "";
            }

            var contentType = ResolveContentTypeFor(result);
            if (!contentType.Contains("xml"))
            {
                return JsonSerializer.Serialize(result.Value);
            }

            var serializer = new XmlSerializer(result.Value.GetType());
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter);
            serializer.Serialize(xmlWriter, result.Value);
            return stringWriter.ToString();
        }
    }
}