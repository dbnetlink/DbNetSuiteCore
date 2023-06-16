using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using System.Data;
using DocumentFormat.OpenXml.Drawing;

namespace DbNetSuiteCore.Components
{
    public class DbNetComboCore : DbNetSuiteCore
    {
        private readonly string _fromPart;
        private readonly string _valueColumn;
        private readonly string _textColumn;

        private List<EventBinding> _eventBindings { get; set; } = new List<EventBinding>();
        private List<DbNetSuiteCore> _linkedControls { get; set; } = new List<DbNetSuiteCore>();
        /// <summary>
        /// Automatically selects the first row of the grid (default is true)
        /// </summary>
        public bool? AutoRowSelect { get; set; } = null;
        /// <summary>
        /// Overrides the default culture that controls default date and currency formatting
        /// </summary>
        public string Culture { get; set; } = null;
        /// <summary>
        /// Automatically adds an empty option to the select 
        /// </summary>
        public bool? AddEmptyOption { get; set; } = null;
        /// <summary>
        /// Adds an input to filter the combo options
        /// </summary>
        public bool? AddFilter { get; set; } = null;
        /// <summary>
        /// Overrides the default culture that controls default date and currency formatting
        /// </summary>
        public string EmptyOptionText { get; set; } = null;
        /// <summary>
        /// Specifies the text for the empty option
        /// </summary>
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
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
        public DbNetComboCore(string connection, string fromPart, string valueColumn, string textColumn = null, string id = null) : base(connection, id)
        {
            _fromPart = fromPart;
            _valueColumn = valueColumn;
            _textColumn = textColumn;
        }

        /// <summary>
        /// Links one grid component to another
        /// </summary>
        public void AddLinkedControl(DbNetSuiteCore linkedControl)
        {
            _linkedControls.Add(linkedControl);
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
            AddProperty(EncodingHelper.Encode(_textColumn), "TextColumn", properties);
            AddProperty(EncodingHelper.Encode(ForeignKeyColumn), nameof(ForeignKeyColumn), properties);
            AddProperty(Size, nameof(Size), properties);
            AddProperty(MultipleSelect, nameof(MultipleSelect), properties);

            if (Params.Count > 0)
            {
                properties.Add($"params = {Serialize(Params)};");
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
