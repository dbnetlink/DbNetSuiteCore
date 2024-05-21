using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;

namespace DbNetSuiteCore.Web.UI.Pages.Samples.DbNetGrid
{
    public class JsonTextModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public string JsonData { get; set; } = string.Empty;

        public JsonTextModel(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public void OnGet()   
        {
            IFileInfo fileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo("/data/employees.json");
            if (fileInfo != null)
            {
                JsonData = System.IO.File.ReadAllText(fileInfo.PhysicalPath ?? string.Empty);
            }
       }
 
    }
}
