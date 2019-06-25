using Microsoft.AspNetCore.Http;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DbNetSuiteCore.Middleware
{
    public class DbNetSuiteJsHandler
    {
        public const string Extension = "dbnetsuitejs";
        public DbNetSuiteJsHandler(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.ContentType = "text/javascript";
            await context.Response.WriteAsync(GetResource("jquery.3.4.1.min.js"));
            await context.Response.WriteAsync(GetResource("bootstrap-notify.min.js"));
            await context.Response.WriteAsync(GetResource("DbNetSuite.js"));
        }

        private string GetResource(string name)
        {
            string text;
            var assembly = typeof(DbNetSuiteJsHandler).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream($"DbNetSuiteCore.JavaScript.{name}"))
            {
                using (var reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            return text;
        }
    }
}
