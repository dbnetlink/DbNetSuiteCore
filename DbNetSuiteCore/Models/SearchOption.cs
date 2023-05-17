namespace DbNetSuiteCore.Models
{
    public class SearchOption
    {
        public string Value { get; set; }
        public string Description { get; set; }
        public string SearchOperator { get; set; }


        public SearchOption(string value, string description, string searchOperator) 
        { 
            Value = value;
            Description = description;
            SearchOperator = searchOperator;
        }
    }
}
