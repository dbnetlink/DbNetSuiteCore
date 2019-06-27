using Microsoft.AspNetCore.Http;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DbNetSuiteCore.Middleware
{
    public class DbNetSuiteJsHandler
    {
        public const string Extension = "dbnetsuitejs";

        private readonly string[] Scripts = { "jquery.3.4.1.min.js", "notify.min.js", "DbNetSuite.js" };
        public DbNetSuiteJsHandler(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.ContentType = "text/javascript";
            foreach (var script in Scripts)
            {
                await context.Response.WriteAsync(GetResource(script));
            }
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
            return $"{text}{System.Environment.NewLine}";
        }
    }
}
