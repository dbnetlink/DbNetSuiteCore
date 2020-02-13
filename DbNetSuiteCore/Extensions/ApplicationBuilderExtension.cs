using DbNetSuiteCore.Middleware;
using DbNetSuiteCore.Services;
using DbNetSuiteCore.Services.Interfaces;
using DbNetSuiteCore.ViewComponents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DbNetSuiteCore.Extensions;


namespace DbNetSuiteCore.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseDbNetSuite(this IApplicationBuilder app)
        {
            app.MapWhen(context => context.Request.Path.ToString().EndsWith(DbNetGridHandler.Extension), UseDbNetGridHandler);
            app.MapWhen(context => context.Request.Path.ToString().EndsWith(DbNetSuiteHandler.Extension), UseDbNetSuiteHandler);

            return app;
        }

        public static IServiceCollection AddDbNetSuite(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
     //       services.AddControllersWithViews()
      //          .AddRazorRuntimeCompilation(options => options.FileProviders.Add(new PhysicalFileProvider(appDirectory)));

            services.AddTransient<IViewRenderService, ViewRenderService>();
            services.AddTransient<IDbNetData, DbNetData>();
            return services;
        }

        public static void UseDbNetGridHandler(this IApplicationBuilder builder)
        {
            //  builder.UseMiddleware<DbNetGridHandler>();
        }
        public static void UseDbNetSuiteHandler(this IApplicationBuilder builder)
        {
            //   builder.UseMiddleware<DbNetSuiteHandler>();
        }
    }
}