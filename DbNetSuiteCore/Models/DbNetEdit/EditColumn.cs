using DbNetSuiteCore.Enums.DbNetEdit;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class EditColumn : DbColumn
    {
        private bool _required = false;
        public EditControlType EditControlType { get; set; } = EditControlType.Auto;
        public string  Pattern { get; set; }
        public bool Hidden => Display == false || ForeignKey || (PrimaryKey && AutoIncrement == true);

        public bool Required
        {
            get => AutoIncrement == false && (AllowsNull == false || PrimaryKey || _required);
            set => _required = value;
        }
        public bool ReadOnly { get; set; } = false;
        public EditColumn()
        {
        }

        public EditColumn(string columnExpression) : base(columnExpression)
        {
        }
    }
}
