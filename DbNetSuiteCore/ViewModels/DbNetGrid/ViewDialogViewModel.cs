﻿using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models.DbNetGrid;
using System.Collections.Generic;
using System.Data;

namespace DbNetSuiteCore.ViewModels.DbNetGrid
{
    public class ViewDialogViewModel : BaseViewModel
    {
        public BooleanDisplayMode BooleanDisplayMode { get; set; }
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public List<GridColumn> Columns { get; set; }
        public DataTable ViewData { get; set; }
        public Dictionary<string, DataTable> LookupTables { get; set; }
        public int LayoutColumns { get; set; }
        public int MaxImageHeight { get; set; }
    }
}
