using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DbNetSuiteCore.Models
{
    ///////////////////////////////////////////////
    public class GridColumnCollection : Collection<GridColumn>
    ///////////////////////////////////////////////
    {
        public GridColumnCollection(IList<GridColumn> list) : base(list)
        {
        }

        public GridColumn this[string ColumnName]
        {
            get
            {
                foreach (GridColumn C in this)
                {
                    if (C.ColumnExpression.ToLower() == ColumnName.ToLower())
                        return C;
                    else if (C.ColumnName.ToLower() == ColumnName.ToLower())
                        return C;
                }
                return null;
            }
        }
    }
}
