﻿using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;

namespace DbNetSuiteCore.Models
{
    public class GridColumn : DbColumn
    {
        public AggregateType Aggregate { get; set; } = AggregateType.None;
        public bool? ClearDuplicateValue { get; set; } = null;
        public bool DataOnly { get; set; } = false;
        public bool Display { get; set; } = true;
        public bool OrderByDescending { get; set; } = false;
        public bool Edit { get; set; } = false;
        public FilterColumnSelectMode FilterMode { get; set; } = FilterColumnSelectMode.Input;
        public bool TotalBreak { get; set; } = false;
        public OrderByDirection? OrderBy { get; set; } = null;
        public string Style { get; set; }
        public bool QuickSearch { get; set; } = false;
        public bool Filter { get; set; } = false;
        public int Index { get; set; } = -1;
        public bool GroupHeader { get; set; } = false;
        public bool Download { get; set; } = false;
        public bool Image { get; set; } = false;
        public bool View { get; set; } = false;
        public bool FixedOrder => GroupHeader || TotalBreak;
        public GridColumn()
        {
        }

        public GridColumn(string columnExpression)
        {
            this.ColumnExpression = EncodingHelper.Encode(columnExpression);
        }
    }
}
