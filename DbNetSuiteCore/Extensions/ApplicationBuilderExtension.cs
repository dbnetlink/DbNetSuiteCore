using DbNetSuiteCore.Middleware;
using DbNetSuiteCore.ViewComponents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace DbNetSuiteCore.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseDbNetSuite(this IApplicationBuilder app)
        {
            app.MapWhen(
               context => context.Request.Path.ToString().EndsWith(DbNetGridHandler.Extension),
               appBranch => {
                   appBranch.UseDbNetGridHandler();
               });

            return app;
        }

        public static IServiceCollection AddDbNetSuite(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.Configure<RazorViewEngineOptions>(options =>
                {
                    options.FileProviders.Add(new EmbeddedFileProvider(typeof(DbNetGridViewComponent).GetTypeInfo().Assembly));
                });

            return services;
        }

        public static IApplicationBuilder UseDbNetGridHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DbNetGridHandler>();
        }
    }
}
