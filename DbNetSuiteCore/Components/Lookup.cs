namespace DbNetSuiteCore.Components
{
    public class Lookup
    {
	    private readonly string _fromPart;
	    private readonly string _foreignKeyColumn;
	    private readonly string _descriptiveColumn;
        private readonly bool _distinct;

        public Lookup(string fromPart, string foreignKeyColumn, string descriptiveColumn = null, bool distinct = false)
        {
	        _fromPart = fromPart;
            _foreignKeyColumn = foreignKeyColumn;
            _descriptiveColumn = descriptiveColumn;
            _distinct = distinct;
        }

        public override string ToString()
        {
	        return $"select {(_distinct ? "distinct " : string.Empty)}{_foreignKeyColumn} {(string.IsNullOrEmpty(_descriptiveColumn) ? null : $",{_descriptiveColumn}")} from {_fromPart}";
        }
    }
}