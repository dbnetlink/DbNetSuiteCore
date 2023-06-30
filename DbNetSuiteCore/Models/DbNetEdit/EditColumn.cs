using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;

namespace DbNetSuiteCore.Models.DbNetGrid
{
    public class EditColumn : DbColumn
    {
      
        public EditColumn()
        {
        }

        public EditColumn(string columnExpression) : base(columnExpression)
        {
        }
    }
}
