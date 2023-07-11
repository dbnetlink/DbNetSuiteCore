namespace DbNetSuiteCore.Components
{
    public class Lookup
    {
	    private readonly string _fromPart;
	    private readonly string _valueColumn;
	    private readonly string _textColumn;
        private readonly bool _distinct;

        public Lookup(string fromPart, string valueColumn, string textColumn = null, bool distinct = false)
        {
	        _fromPart = fromPart;
	        _valueColumn = valueColumn;
			_textColumn = textColumn;
            _distinct = distinct;
        }

        public override string ToString()
        {
	        return $"select {(_distinct ? "distinct " : string.Empty)}{_valueColumn} {(string.IsNullOrEmpty(_textColumn) ? null : $",{_textColumn}")} from {_fromPart}";
        }
    }
}