using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using System.Data;
using DbNetSuiteCore.Enums.DbNetEdit;

namespace DbNetSuiteCore.Components
{
    public class DbNetEditCore : DbNetSuiteCore
    {
        private readonly string _fromPart; 

        private List<EventBinding> _eventBindings { get; set; } = new List<EventBinding>();
  
        /// <summary>
        /// Specifies the name of the foreign key column in a linked combo
        /// </summary>
        public string ForeignKeyColumn { get; set; } = null;
        /// <summary>
        /// Adds/removes a page navigation to/from the toolbar
        /// </summary>
        public bool? Navigation { get; set; } = null;
        /// <summary>
        /// Displays a search box in the toolbar that allows for searching against all the text based columns
        /// </summary>
        public bool? QuickSearch { get; set; } = null;
        /// <summary>
        /// Adds/removes a search dialog option to/from the toolbar
        /// </summary>
        public bool? Search { get; set; } = null;
        public DbNetEditCore(string connection, string fromPart, string id = null) : base(connection, id)
        {
            _fromPart = fromPart;
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
            AddProperty(EncodingHelper.Encode(ForeignKeyColumn), nameof(ForeignKeyColumn), properties);
            return string.Join(Environment.NewLine, properties);
        }
    }
}
