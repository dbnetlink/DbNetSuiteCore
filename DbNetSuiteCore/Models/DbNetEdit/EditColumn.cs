using DbNetSuiteCore.Enums.DbNetEdit;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class EditColumn : DbColumn
    {
        public EditControlType EditControlType { get; set; } = EditControlType.Auto;
        public string  Pattern { get; set; }
        public bool Browse { get; set; } = false;

        public EditColumn()
        {
        }

        public EditColumn(string columnExpression) : base(columnExpression)
        {
        }
    }
}
