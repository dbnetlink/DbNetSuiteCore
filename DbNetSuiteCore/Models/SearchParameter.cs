using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.Models
{
    public class SearchParameter
    {
        public SearchOperator SearchOperator { get; set; }
        public int ColumnIndex { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public bool Value1Valid { get; set; } = true;
        public bool Value2Valid { get; set; } = true;
    }
}
