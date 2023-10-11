using DbNetSuiteCore.Helpers;
using System.Linq;

namespace DbNetSuiteCore.Components
{
    public class Lookup
    {
        private readonly string _sql;
        private readonly string _fromPart;
	    private readonly string _foreignKeyColumn;
	    private readonly string _descriptiveColumn;
        private readonly bool _distinct;
        private readonly string _parameter;
        public string Parameter => _parameter?.Replace("@",string.Empty).ToLower();

        public Lookup(string fromPart, string foreignKeyColumn, string descriptiveColumn = null, bool distinct = false)
        {
	        _fromPart = fromPart;
            _foreignKeyColumn = foreignKeyColumn;
            _descriptiveColumn = descriptiveColumn;
            _distinct = distinct;
        }

        public Lookup(string sql)
        {
            _sql = sql;
            _parameter = TextHelper.ParseParameter(sql);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_sql))
            {
                return $"select {(_distinct ? "distinct " : string.Empty)}{_foreignKeyColumn} {(string.IsNullOrEmpty(_descriptiveColumn) ? null : $",{_descriptiveColumn}")} from {_fromPart}";
            }
            else
            {
                return _sql;
            }
        }
    }
}