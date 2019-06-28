using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;

namespace DbNetSuiteCore.Services.Interfaces
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName);
        Task<string> RenderToStringAsync<TModel>(string viewName, TModel model);
        string RenderToString<TModel>(string viewPath, TModel model);
        string RenderToString(string viewPath);
    }
}
