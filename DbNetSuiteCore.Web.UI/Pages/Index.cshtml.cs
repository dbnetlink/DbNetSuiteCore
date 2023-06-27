using DbNetSuiteCore.Web.UI.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;

namespace DbNetSuiteCore.Web.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Dictionary<ComponentEnum,IDirectoryContents> TestPages { get; set; } = new Dictionary<ComponentEnum,IDirectoryContents>();

        public IndexModel(IWebHostEnvironment webHostEnvironment) 
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public void OnGet()
        {
            foreach (ComponentEnum component in Enum.GetValues(typeof(ComponentEnum)))
            {
                TestPages[component] = _webHostEnvironment.ContentRootFileProvider.GetDirectoryContents($"pages/{component}");
            }
        }
    }
}
