using DbNetSuiteCore.Helpers;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DbNetSuiteCore.Middleware
{
    public class DbNetSuiteHandler
    {
        public const string Extension = "dbnetsuite";

        public DbNetSuiteHandler(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            var handler = (string)context.Request.Query["handler"] ?? string.Empty;
            var content = string.Empty;

            switch (handler.ToLower())
            {
                case "css":
                    content = ResourceHelper.GetResource("Css.DbNetSuite.css");
                    context.Response.ContentType = "text/css";
                    break;
                case "js":
                    content = ResourceHelper.GetResource("JavaScript.DbNetSuite.js");
                    context.Response.ContentType = "text/javascript";
                    break;
            }

            await context.Response.WriteAsync(content);
        }
    }
}
