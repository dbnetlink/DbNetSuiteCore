using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using DbNetSuiteCore.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DbNetSuiteCore.Services
{
    public class DbNetSuite
    {
        protected string Action => QueryParam("action");

        protected readonly HttpContext HttpContext;
        protected readonly IWebHostEnvironment Env;
        protected readonly IConfiguration Configuration;

        protected readonly DbNetSuiteCoreSettings Settings; 

        public DbNetSuite(AspNetCoreServices services)
        {
            HttpContext = services.httpContext;
            Env = services.webHostEnvironment;
            Configuration = services.configuration;
            Settings = services.configuration.GetSection("DbNetSuiteCore").Get<DbNetSuiteCoreSettings>();
        }
        protected string QueryParam(string name)
        {
            return HttpContext.Request.Query[name];
        }

        protected void ThrowException(string Msg, string Info = null)
        {
        }

        protected async Task<string> GetResourceString(string resourceName)
        {
            byte[] buffer = await GetResource(resourceName);
            return new UTF8Encoding(false).GetString(buffer);
      //      return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }
        protected async Task<byte[]> GetResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream($"DbNetSuiteCore.Resources.{resourceName}"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    byte[] buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
        }
    }
}

