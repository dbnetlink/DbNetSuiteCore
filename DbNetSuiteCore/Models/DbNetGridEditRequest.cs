namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class DbNetGridEditRequest : DbNetSuiteRequest
    {
        public string FromPart { get; set; }
        public bool Navigation { get; set; } = true;
        public bool QuickSearch { get; set; } = false;
        public bool Search { get; set; } = true;
    }
}