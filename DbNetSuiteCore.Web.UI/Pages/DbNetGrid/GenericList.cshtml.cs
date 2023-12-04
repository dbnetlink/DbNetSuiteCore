using DbNetSuiteCore.Web.UI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;

namespace DbNetSuiteCore.Web.UI.Pages.Samples.DbNetGrid
{
    public class GenericListModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public List<Employee> Employees { get; set; }

        public GenericListModel(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public void OnGet()
        {
            IFileInfo fileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo("/data/employees.json");
            if (fileInfo != null)
            {
                string json = System.IO.File.ReadAllText(fileInfo.PhysicalPath);
                Employees = JsonSerializer.Deserialize<List<Employee>>(json);
            }
        }

    }
}
