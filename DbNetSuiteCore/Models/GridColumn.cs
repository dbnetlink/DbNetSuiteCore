using DbNetSuiteCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbNetSuiteCore.Models
{
    /////////////////////////////////////////////// 
    public class GridColumn : DbColumn
    ///////////////////////////////////////////////
    {
        public enum LookupSearchModeValues
        {
            SearchValue,
            SearchText
        }

        public AuditModes Audit { get; set; } = AuditModes.None;
        internal Dictionary<string, string> LookupData = new Dictionary<string, string>();
        public bool DropDownFilter { get; set; } = false;

        public GridColumn(string columnName) : base(columnName)
        {
        }
    }
}
