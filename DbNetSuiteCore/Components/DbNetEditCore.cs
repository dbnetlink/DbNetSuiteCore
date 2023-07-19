using DbNetSuiteCore.Helpers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using DbNetSuiteCore.Enums.DbNetEdit;
using System;

namespace DbNetSuiteCore.Components
{
    public class DbNetEditCore : DbNetGridEditCore
    {
        /// <summary>
        /// Specifies the name of the foreign key column in a linked combo
        /// </summary>
        public string ForeignKeyColumn { get; set; } = null;
        internal bool? IsEditDialog { get; set; } = null;
        /// <summary>
        /// Specifies the number of columns over which the edit fields are distributed
        /// </summary>
        public int? LayoutColumns { get; set; }

        public DbNetEditCore(string connection, string fromPart, string id = null) : base(connection, fromPart, id)
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
        /// Assigns a grid column property to multiple columns
        /// </summary>
        public void SetColumnProperty(string[] columnNames, ColumnPropertyType propertyType, object propertyValue)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnProperty(columnName, propertyType, propertyValue);
            }
        }
        /// <summary>
        /// Assigns a grid column property value to a single column
        /// </summary>
        public void SetColumnProperty(string columnName, ColumnPropertyType propertyType, object propertyValue)
        {
            base.SetColumnProperty(columnName, propertyType, (object)propertyValue);
        }

        /// <summary>
        /// Sets the edit control type for the specified column
        /// </summary>
        public void SetControlType(string columnName, object propertyValue)
        {
            SetColumnProperty(columnName, ColumnPropertyType.EditControlType, (object)propertyValue);
        }
        public HtmlString Render()
        {
            string message = ValidateProperties();

            if (string.IsNullOrEmpty(message) == false)
            {
                return new HtmlString($"<div class=\"dbnetsuite-error\">{message}</div>");
            }

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

        private string Markup()
        {
            return _idSupplied ? string.Empty : $"<section id=\"{_id}\"></section>";
        }
        private string Properties()
        {
            List<string> properties = new List<string>();
            AddProperty(LayoutColumns, nameof(LayoutColumns), properties);
            AddProperty(IsEditDialog, nameof(IsEditDialog), properties);

            AddProperties(properties);

            //AddProperty(EncodingHelper.Encode(FixedFilterSql), nameof(FixedFilterSql), properties);
            properties.Add($"datePickerOptions = {DatePickerOptions()};");

            return string.Join(Environment.NewLine, properties);
        }
    }
}
