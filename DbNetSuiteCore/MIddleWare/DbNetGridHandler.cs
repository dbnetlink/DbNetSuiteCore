using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace DbNetSuiteCore.Middleware
{
    public class DbNetGridHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _env;
        private DbNetGridConfiguration _DbNetGridConfiguration;
        private readonly IConfiguration _configuration;
        public const string Extension = ".dbnetgrid";
        // Must have constructor with this signature, otherwise exception at run time
        public DbNetGridHandler(RequestDelegate next, 
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment env) 
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            _DbNetGridConfiguration = SerialisationHelper.DeserialiseJson<DbNetGridConfiguration>(context.Request.Body);

            var connectionString = _configuration.GetConnectionString(_DbNetGridConfiguration.ConnectionString);

            string response = GenerateResponse(context);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(response);
        }

        private string GenerateResponse(HttpContext context)
        {
            return "{}";
        }
    }
}
