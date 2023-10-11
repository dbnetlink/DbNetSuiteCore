﻿using DbNetSuiteCore.Helpers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using DbNetSuiteCore.Enums.DbNetEdit;
using System;
using System.Linq;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;

namespace DbNetSuiteCore.Components
{
    public class DbNetEditCore : DbNetGridEditCore
    {
        internal bool _browse => _columnProperties.Any(c => c.PropertyType is ColumnPropertyType.Browse);
        internal string _browseDialogId;
        internal bool? IsEditDialog { get; set; } = null;
        /// <summary>
        /// Specifies the number of columns over which the edit fields are distributed
        /// </summary>
        public int? LayoutColumns { get; set; }

        public DbNetEditCore(string connection, string fromPart, string id = null) : base(connection, fromPart, id)
        {
            BrowseControl = new DbNetGridCore(connection, fromPart, true);
            this._browseDialogId = $"{this.Id}_browse_dialog";
        }

        internal DbNetEditCore(string connection, string fromPart, bool editControl) : base(connection, fromPart)
        {
 
        }
        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(EventType eventType, string functionName)
        {
            base.Bind(eventType, functionName);
        }
        /// <summary>
        /// Browse control
        /// </summary>
        public DbNetGridCore BrowseControl { get; set; }
        /// <summary>
        /// Sets the type of edit control type for the column
        /// </summary>
        public void SetControlType(string columnName, EditControlType editControlType)
        {
            SetColumnProperty(columnName, ColumnPropertyType.EditControlType, editControlType);
        }
        /// <summary>
        /// Sets the type of edit control type for the columns
        /// </summary>
        public void SetControlType(string[] columnNames, EditControlType editControlType)
        {
            foreach (string columnName in columnNames)
            {
                SetControlType(columnName, editControlType);
            }
        }
        /// <summary>
        /// Sets the size of the edit field
        /// </summary>
        public void SetColumnSize(string columnName, int size)
        {
            SetColumnProperty(columnName, ColumnPropertyType.ColumnSize, size);
        }
        /// <summary>
        /// Sets the size of the edit field
        /// </summary>
        public void SetColumnPattern(string columnName, string pattern)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Pattern, pattern);
        }
        /// <summary>
        /// Specifies the columns that will be displayed in the browse dialog
        /// </summary>
        public void SetColumnBrowse(string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                SetColumnProperty(columnName, ColumnPropertyType.Browse, true);
            }
        }
        /// <summary>
        /// Indicates the columns that are required in the edit form
        /// </summary>
        public void SetColumnRequired(string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                SetColumnProperty(columnName, ColumnPropertyType.Required, true);
            }
        }
        /// <summary>
        /// Disables the ability to modify the fields in the form
        /// </summary>
        public void SetColumnReadOnly(string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                SetColumnReadOnly(columnName);
            }
        }
        /// <summary>
        /// Disables the ability to modify the field in the form
        /// </summary>
        public void SetColumnReadOnly(string columnName)
        {
            SetColumnProperty(columnName, ColumnPropertyType.ReadOnly, true);
        }
        public HtmlString Render()
        {
            string message = ValidateProperties();

            if (string.IsNullOrEmpty(message) == false)
            {
                return new HtmlString($"<div class=\"dbnetsuite-error\">{message}</div>");
            }

            AddBrowseControl();

            string script = string.Empty;

            script += @$" 
                {Markup()}
                <script type=""text/javascript"">
                {ClientJavaScript()}
                </script>";

            return new HtmlString(script);
        }

        internal string LinkedRender()
        {
            var script = @$" 
{ConfigureLinkedControls()}
var {_id} = new DbNetEdit('{_id}');
with ({_id})
{{
	{EditConfiguration()}                        
}};";
            return script;
        }

        private string ClientJavaScript()
        {
            var script = @$"{InitScript()}
	{ConfigureLinkedControls()}
	var {_id} = new DbNetEdit('{_id}');
	with ({_id})
	{{
		{EditConfiguration()}                            
		initialize();
	}}
}};";

            if (GetCurrentSettings().Debug == false)
            {
                script = script.Replace("\r\n", string.Empty).Replace("\t", string.Empty);
            }
            return script;
        }

        private string EditConfiguration()
        {
            var script = @$" 
connectionString = '{EncodingHelper.Encode(_connection)}';
fromPart = '{EncodingHelper.Encode(_fromPart)}';
{ColumnExpressions()}
{ColumnKeys()}
{ColumnLabels()}
{ColumnProperties()}
{EventBindings()}
{Properties()}
{LinkedControls()}";
            return script;
        }


        private string Properties()
        {
            List<string> properties = new List<string>();
            AddProperty(LayoutColumns, nameof(LayoutColumns), properties);
            AddProperty(IsEditDialog, nameof(IsEditDialog), properties);
            AddProperty(_browseDialogId, "BrowseDialogId", properties);
            AddProperties(properties);

            properties.Add($"datePickerOptions = {DatePickerOptions()};");

            return string.Join(Environment.NewLine, properties);
        }

        private void AddBrowseControl()
        {
            if (_browse)
            {
                BrowseControl.AutoRowSelect = false;
                BrowseControl.PageSize = -1;
                BrowseControl.ToolbarPosition = Enums.ToolbarPosition.Hidden;
                BrowseControl.Columns = new List<string>(this.Columns.ToArray());
                BrowseControl.Labels = new List<string>(this.Labels.ToArray());

                var browseColumns = this._columnProperties.Where(c => c.PropertyType is ColumnPropertyType.Browse).Select(c => c.ColumnName).ToList();

                foreach (ColumnProperty columnProperty in this._columnProperties.Where(c => browseColumns.Contains(c.ColumnName.ToLower())))
                {
                    if (BrowseControl._columnProperties.Any(p => p.Equals(columnProperty)) == false)
                    {
                        BrowseControl._columnProperties.Add(columnProperty);
                    }
                }

                this._linkedControls.Add(BrowseControl);
            }
            else
            {
                _browseDialogId = null;
                BrowseControl = null;
            }
        }
    }
}
