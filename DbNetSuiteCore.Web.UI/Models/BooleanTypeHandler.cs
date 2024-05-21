using Dapper;
using System.Data;

namespace DbNetSuiteCore.Web.UI.Models
{
    public class BooleanTypeHandler : SqlMapper.TypeHandler<bool>
    {
        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            parameter.Value = value;
        }
        public override bool Parse(object value)
        {
            return Convert.ToBoolean(Convert.ToInt16(value));
        }
    }
  
}
