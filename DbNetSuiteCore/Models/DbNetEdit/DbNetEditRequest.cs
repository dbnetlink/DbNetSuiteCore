using DbNetSuiteCore.Enums;
using System;
using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class DbNetEditRequest : DbNetGridEditRequest
    {
        public Dictionary<string,object> Changes { get; set; } = new Dictionary<string, object>();
        public List<EditColumn> Columns { get; set; } = new List<EditColumn>();
        public long CurrentRow { get; set; } = 1;
        public int LayoutColumns { get; set; } = 1;
        public long TotalRows { get; set; }
        public bool IsEditDialog { get; set; } = false;
        public ToolbarPosition ToolbarPosition { get; set; } = ToolbarPosition.Bottom;
        public Guid FormCacheKey { get; set; }
    }
}