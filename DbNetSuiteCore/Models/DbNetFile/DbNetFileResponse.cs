using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetFile
{
    public class DbNetFileResponse : DbNetSuiteResponse
    {
        public int TotalPages { get; set; }
        public int TotalRows { get; set; }
        public int CurrentPage { get; set; }
        public List<SearchParameter> SearchParams { get; set; }
    }
}