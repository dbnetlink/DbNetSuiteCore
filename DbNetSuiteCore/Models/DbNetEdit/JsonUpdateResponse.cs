using System.Collections.Generic;

namespace DbNetSuiteCore.Web.UI.Models
{
    public class JsonUpdateResponse
    {
        public bool? Success { get; set; }
        public string? Message { get; set; }
        public object? DataSet { get; set; }
    };
}
