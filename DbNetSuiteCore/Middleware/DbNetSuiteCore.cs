using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Services;
using DocumentFormat.OpenXml.InkML;
using System.Net;
using DbNetSuiteCore.Constants.DbNetGrid;
using Microsoft.Extensions.Caching.Memory;

namespace DbNetLink.Middleware
{
    public class DbNetSuiteCore
    {
        private RequestDelegate _next;


        public DbNetSuiteCore(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env, IConfiguration configuration, IMemoryCache cache)
        {
            if (context.Request.Path.ToString().EndsWith(DbNetSuiteExtensions.PathExtension))
            {
                await GenerateResponse(new AspNetCoreServices(context, env, configuration, cache));
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private async Task GenerateResponse(AspNetCoreServices services)
        {
            string page = services.httpContext.Request.Path.ToString().Split('/').Last().Split('.').First();
            string action = services.httpContext.Request.Query["name"];
            object response = null;

            switch (page.ToLower())
            {
                case "resource":
                    var resource = new Resource(services);
                    response = await resource.Process();
                    break;
                case "dbnetgrid":
                    var dbnetgrid = new DbNetGrid(services);
                    response = await dbnetgrid.Process();
                    break;
                case "dbnetcombo":
                    var dbnetcombo = new DbNetCombo(services);
                    response = await dbnetcombo.Process();
                    break;
                case "dbnetedit":
                    var dbnetedit = new DbNetEdit(services);
                    response = await dbnetedit.Process();
                    break;
                case "dbnetsuite":
                    var dbnetsuite = new DbNetSuite(services);
                    response = await dbnetsuite.Process();
                    break;
                case "dbnetfile":
                    var dbnetfile = new DbNetFile(services);
                    response = await dbnetfile.Process();
                    break;
            }

            if (response == null)
            {
                services.httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await services.httpContext.Response.CompleteAsync();
            }
            else if (response is byte[])
            {
                await services.httpContext.Response.Body.WriteAsync(response as byte[]);
            }
            else
            {
                await services.httpContext.Response.WriteAsync(response.ToString());
            }
        }
    }

    public static class DbNetSuiteExtensions
    {
        public static string PathExtension => ".dbnetsuite";

        public static IApplicationBuilder UseDbNetSuiteCore(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DbNetSuiteCore>();
        }

        public static IServiceCollection AddDbNetSuiteCore(this IServiceCollection services)
        {
            services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                options.FileProviders.Clear();
                var embeddedFileProvider = new EmbeddedFileProvider(typeof(DbNetSuiteCore).Assembly);
                options.FileProviders.Add(embeddedFileProvider);
            });
            services.AddRazorPages().AddRazorRuntimeCompilation();
            return services;
        }
    }
}