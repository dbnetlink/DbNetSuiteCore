using DbNetSuiteCore.Components;
using DbNetSuiteCore.Enums.DbNetEdit;
using DbNetSuiteCore.Helpers;
using System.ComponentModel;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class EditColumn : DbColumn
    {
        private bool _required = false;
        private string _pattern;
        public EditControlType EditControlType { get; set; } = EditControlType.Auto;
        public string EditControlTypeName => (EditControlType.GetAttribute<DescriptionAttribute>()?.Description ?? EditControlType.ToString()).ToLower();
        public bool Hidden => Display == false || ForeignKey || (PrimaryKey && AutoIncrement == true);
        public InputValidation InputValidation { get; set; }
        public string Pattern
        {
            get => string.IsNullOrEmpty(_pattern) ? InputValidation?.Pattern : _pattern;
            set => _pattern = value;
        }
        public string Placeholder { get; set; }
        public bool Required
        {
            get => AutoIncrement == false && (AllowsNull == false || PrimaryKey || _required || (InputValidation?.Required ?? false));
            set => _required = value;
        }
        public bool ReadOnly { get; set; } = false;

        public string Annotation { get; set; }
        public TextTransform? TextTransform { get; set; }

        public EditColumn()
        {
        }

        public EditColumn(string columnExpression) : base(columnExpression)
        {
        }
    }
}
