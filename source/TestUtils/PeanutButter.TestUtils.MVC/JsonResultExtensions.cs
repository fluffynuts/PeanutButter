using System;
using System.Web.Mvc;

namespace PeanutButter.TestUtils.MVC
{
    public static class JsonResultExtensions
    {
        public static T Get<T>(this JsonResult jsonResult, string propName = null)
        {
            var data = jsonResult.Data;
            if (propName == null)
                return (T)data;
            var prop = data.GetType().GetProperty(propName);
            if (prop == null) throw new Exception(string.Join(string.Empty, new[] { "Unable to find property '", propName, "' on provided jsonResult" }));
            return (T)prop.GetValue(data, null);
        }
    }
}
