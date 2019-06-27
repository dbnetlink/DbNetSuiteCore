using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DbNetSuiteCore.Models.Configuration;

namespace DbNetSuiteCore.ViewComponents
{
    public class DbNetGridViewComponent : ViewComponent
    {
        public DbNetGridViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(DbNetGridConfiguration configuration)
        {
            return View(configuration);
        }
    }
}
