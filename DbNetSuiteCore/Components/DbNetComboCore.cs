using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data;

namespace DbNetSuiteCore.Components
{
    public class DbNetComboCore : DbNetSuiteCore
    {
        private readonly string _sql;
        private List<EventBinding> _eventBindings { get; set; } = new List<EventBinding>();
        private List<DbNetSuiteCore> _linkedControls { get; set; } = new List<DbNetSuiteCore>();
        public string Id => _id;
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

        public DbNetComboCore(string connection, string sql, string id = null): base(connection, id)
        {
            _sql = sql;
        }
      
        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(EventType eventType, string functionName)
        {
            _eventBindings.Add(new EventBinding(eventType, functionName));
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

        private string ClientJavaScript()
        {
            var script = @$" 
document.addEventListener('DOMContentLoaded', function() {{init_{_id}()}});
function init_{_id}()
{{
    if (typeof(DbNetGrid) == 'undefined') {{alert('DbNetSuite client-side code has not loaded. Add @DbNetSuiteCore.ClientScript() to your razor page. See console for details');console.error(""DbNetSuite stylesheet not found. See https://dbnetsuitecore.z35.web.core.windows.net/index.htm?context=20#DbNetSuiteCoreClientScript"");return;}};
	var {_id} = new DbNetCombo('{_id}');
	with ({_id})
	{{
		{ComboConfiguration()}                            
		initialize();
	}}
}}";

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
sql = '{EncodingHelper.Encode(_sql)}';
{EventBindings()}
{Properties()}";
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
            AddProperty(EmptyOptionText, nameof(EmptyOptionText), properties);
            AddProperty(AddFilter, nameof(AddFilter), properties);

            if (Params.Count > 0)
            {
                properties.Add($"params = {Serialize(Params)};");
            }

            return string.Join(Environment.NewLine, properties);
        }
          private string EventBindings()
        {
            var script = new List<string>();
            script = _eventBindings.Select(x => $"bind(\"{LowerCaseFirstLetter(x.EventType.ToString())}\",{x.FunctionName});").ToList();
            return string.Join(Environment.NewLine, script);
        }
    }
}
