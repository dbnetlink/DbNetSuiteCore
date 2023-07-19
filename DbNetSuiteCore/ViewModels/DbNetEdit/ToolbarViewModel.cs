﻿using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.ViewModels.DbNetEdit
{
    public class ToolbarViewModel : BaseViewModel
    {
        public bool QuickSearch { get; set; }
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public bool Search { get; set; }
        public bool Navigation { get; set; }
        public bool IsEditDialog { get; set; }
        public bool Insert { get; set; }
        public bool Delete { get; set; }
    }
}
