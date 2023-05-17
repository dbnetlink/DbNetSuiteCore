using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using DbNetSuiteCore.Extensions;

namespace Microsoft.AspNetCore.Mvc
{
    public static class DbNetLink
    {
        public static async Task<string> RenderToStringAsync(this HttpContext httpContext, string viewName, object model)
        {
            var viewEngine = httpContext.RequestServices.GetRequiredService<IRazorViewEngine>();
            var viewOptions = httpContext.RequestServices.GetRequiredService<IOptions<MvcViewOptions>>();
            var engine = new RazorEngine(viewEngine, viewOptions);
            return await engine.RenderAsync(httpContext, viewName, model);
        }

        public static async Task RenderAsync(this HttpContext httpContext, string viewName, object model)
        {
            var viewEngine = httpContext.RequestServices.GetRequiredService<IRazorViewEngine>();
            var viewOptions = httpContext.RequestServices.GetRequiredService<IOptions<MvcViewOptions>>();
            var engine = new RazorEngine(viewEngine, viewOptions);
            await engine.RenderAsync(httpContext, viewName, model, httpContext.Response.Body);
        }
    }
}