using DbNetSuiteCore.Services;
using System.ComponentModel;
using System.Resources;

namespace DbNetSuiteCore.ViewModels
{
    public class BaseViewModel
    {
        public string ComponentId { get; set; }
        public DbNetSuite Component { get; set; }

        public ResourceManager ResourceManager { get; set; }

        public string Translate(string key)
        {
            return Component.Translate(key);
        }
    }
}