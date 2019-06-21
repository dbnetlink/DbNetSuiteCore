using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DbNetSuiteCore.ViewComponents
{
    public class DbNetGridViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _env;
        public DbNetGridViewComponent(
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment env
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
