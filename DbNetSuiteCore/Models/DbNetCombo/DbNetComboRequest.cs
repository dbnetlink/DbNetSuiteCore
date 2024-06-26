﻿using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetCombo
{
    public class DbNetComboRequest : DbNetSuiteRequest
    {
        public List<string> DataOnlyColumns { get; set; } = new List<string>();
        public string FromPart { get; set; }
        public string ValueColumn { get; set; }
        public string TextColumn { get; set; }
        public bool AddEmptyOption { get; set; } = false;
        public bool AddFilter { get; set; } = false;
        public bool Distinct { get; set; } = false;
        public string EmptyOptionText { get; set; } = string.Empty;
        public Dictionary<string, object> ProcedureParams { get; set; } = new Dictionary<string, object>();
        public string FilterToken { get; set; } = string.Empty;
        public string ForeignKeyColumn { get; set; } = null;
        public List<object> ForeignKeyValue { get; set; } = null;
        public int Size { get; set; } = 1;
        public bool MultipleSelect { get; set; } = false;
        public string ProcedureName { get; set; } = null;
    }
}