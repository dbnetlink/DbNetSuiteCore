using DbNetSuiteCore.Helpers;
using System;
using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class DbNetEditResponse : DbNetGridEditResponse
    {
        private List<EditColumn> columns;
        private string primaryKey;

        public object Toolbar { get; set; }
        public object Form { get; set; }
        public List<EditColumn> Columns
        {
            get { return columns; }
            set { columns = value; columns.ForEach(c => c.EncodeClientProperties()); }
        }
        public long TotalRows { get; set; }
        public long CurrentRow { get; set; }
        public string PrimaryKey
        {
            get { return primaryKey; }
            set { primaryKey = EncodingHelper.Encode(value); }
        }

        public KeyValuePair<string, string>? ValidationMessage { get; set; } = null;
        public Guid FormCacheKey { get; set; }
        public string ConvertedDate { get; set; }
    }
}