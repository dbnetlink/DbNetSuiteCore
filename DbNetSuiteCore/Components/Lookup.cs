namespace DbNetSuiteCore.Components
{
    public class Lookup
    {
	    private readonly string _fromPart;
	    private readonly string _valueColumn;
	    private readonly string _textColumn;

        public Lookup(string fromPart, string valueColumn, string textColumn)
        {
	        _fromPart = fromPart;
	        _valueColumn = valueColumn;
			_textColumn = textColumn;
		}

        public override string ToString()
        {
	        return $"select {_valueColumn} {(string.IsNullOrEmpty(_textColumn) ? null : $",{_textColumn}")} from {_fromPart}";
        }
    }
}