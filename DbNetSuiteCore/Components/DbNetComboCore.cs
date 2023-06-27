using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using System.Data;
using DocumentFormat.OpenXml.Drawing;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Enums.DbNetCombo;

namespace DbNetSuiteCore.Components
{
    public class DbNetComboCore : DbNetSuiteCore
    {
        private readonly string _fromPart; 
        private readonly string _valueColumn;
        private readonly string _textColumn;
        private readonly string _procedureName;

        private List<EventBinding> _eventBindings { get; set; } = new List<EventBinding>();
        private List<DbNetSuiteCore> _linkedControls { get; set; } = new List<DbNetSuiteCore>();
        /// <summary>
        /// Automatically selects the first row of the combo. Only applicable where Size is greater than 1.
        /// </summary>
        public bool? AutoRowSelect { get; set; } = null;
        /// <summary>
        /// Automatically adds an empty option to the select 
        /// </summary>
        public bool? AddEmptyOption { get; set; } = null;
        /// <summary>
        /// Adds an input to filter the combo options
        /// </summary>
        public bool? AddFilter { get; set; } = null;
        /// <summary>
        /// Specifies columns whose values will be added as data attributes to each option in the combo
        /// </summary>
        public List<string> DataOnlyColumns { get; set; } = new List<string>();
        /// <summary>
        /// Selects distinct (unique) values only 
        /// </summary>
        public bool? Distinct { get; set; } = null;        
        /// <summary>
        /// Specifies the text for the empty option
        /// </summary>
        public string EmptyOptionText { get; set; } = null;
        /// <summary>
        /// Specifies any parameters values when the data source is a stored procedure
        /// </summary>
        public Dictionary<string, object> ProcedureParams { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// Specifies the name of the foreign key column in a linked combo
        /// </summary>
        public string ForeignKeyColumn { get; set; } = null;
        /// <summary>
        /// Specifies the number of visible options in the list
        /// </summary>
        public int? Size { get; set; } = null;
        /// <summary>
        /// Allows the selection of multiple options
        /// </summary>
        public bool? MultipleSelect { get; set; } = null;
        public DbNetComboCore(string connection, string fromPart, string valueColumn, string textColumn = null, string id = null, DataSourceType dataSourceType = DataSourceType.TableOrView) : base(connection, id)
        {
            _fromPart = fromPart;
            _valueColumn = valueColumn;
            _textColumn = textColumn;
            if (dataSourceType == DataSourceType.StoredProcedure)
            {
                _procedureName = fromPart;
            }
        }

        /// <summary>
        /// Links one grid component to another
        /// </summary>
        public void AddLinkedControl(DbNetSuiteCore linkedControl)
        {
            _linkedControls.Add(linkedControl);
        }

        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(EventType eventType, string functionName)
        {
            base.Bind(eventType, functionName);
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
var {_id} = new DbNetCombo('{_id}');
with ({_id})
{{
	{ComboConfiguration()}                        
}};";
            return script;
        }

        private string ClientJavaScript()
        {
            var script = @$"{InitScript()}
	{ConfigureLinkedControls()}
	var {_id} = new DbNetCombo('{_id}');
	with ({_id})
	{{
		{ComboConfiguration()}                            
		initialize();
	}}
}};";

            if (GetCurrentSettings().Debug == false)
            {
                script = script.Replace("\r\n", string.Empty).Replace("\t", string.Empty);
            }
            return script;
        }

        private string ComboConfiguration()
        {
            var script = @$" 
connectionString = '{EncodingHelper.Encode(_connection)}';
fromPart = '{EncodingHelper.Encode(_fromPart)}';
valueColumn = '{EncodingHelper.Encode(_valueColumn)}';

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
            AddProperty(AddEmptyOption, nameof(AddEmptyOption), properties);
            AddProperty(AutoRowSelect, nameof(AutoRowSelect), properties);
            AddProperty(EmptyOptionText, nameof(EmptyOptionText), properties);
            AddProperty(AddFilter, nameof(AddFilter), properties);
            AddProperty(Distinct, nameof(Distinct), properties);
            AddProperty(EncodingHelper.Encode(_textColumn), "TextColumn", properties);
            AddProperty(EncodingHelper.Encode(ForeignKeyColumn), nameof(ForeignKeyColumn), properties);
            AddProperty(Size, nameof(Size), properties);
            AddProperty(MultipleSelect, nameof(MultipleSelect), properties);
            AddProperty(EncodingHelper.Encode(_procedureName), "ProcedureName", properties);

            if (DataOnlyColumns.Count > 0)
            {
                DataOnlyColumns = DataOnlyColumns.Select(c => { c = EncodingHelper.Encode(c);return c;}).ToList();
                properties.Add($"dataOnlyColumns = {Serialize(DataOnlyColumns)};");
            }

            if (ProcedureParams?.Count > 0)
            {
                properties.Add($"procedureParams = {Serialize(ProcedureParams)};");
            }

            return string.Join(Environment.NewLine, properties);
        }

        private string LinkedControls()
        {
            var script = new List<string>();
            script = _linkedControls.Select(x => $"addLinkedControl({x.Id});").ToList();
            return string.Join(Environment.NewLine, script);
        }

        private string ConfigureLinkedControls()
        {
            var script = string.Empty;

            foreach (var linkedControl in _linkedControls)
            {
                if (linkedControl is DbNetGridCore)
                {
                    script += (linkedControl as DbNetGridCore).LinkedRender();
                }
                if (linkedControl is DbNetComboCore)
                {
                    script += (linkedControl as DbNetComboCore).LinkedRender();
                }
            }

            return script;
        }
    }
}
