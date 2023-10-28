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
    public class DbNetFileCore : DbNetSuiteCore
    {
        private readonly string _folder;
        public DbNetFileCore(string folder, string id = null) : base(id)
        {
            _folder = folder;
        }
        public string Folder => _folder;

        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(EventType eventType, string functionName)
        {
            base.BindEvent(eventType, functionName);
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
var {_id} = new DbNetFile('{_id}');
with ({_id})
{{
	{FileConfiguration()}                        
}};";
            return script;
        }

        private string ClientJavaScript()
        {
            var script = @$"{InitScript()}
	{ConfigureLinkedControls()}
	var {_id} = new DbNetFile('{_id}');
	with ({_id})
	{{
		{FileConfiguration()}                            
		initialize();
	}}
}};";

            if (GetCurrentSettings().Debug == false)
            {
                script = script.Replace("\r\n", string.Empty).Replace("\t", string.Empty);
            }
            return script;
        }

        private string FileConfiguration()
        {
            var script = @$" 
folder = '{EncodingHelper.Encode(_folder)}';
{EventBindings()}
{Properties()}
{LinkedControls()}";
            return script;
        }

        private string Properties()
        {
            List<string> properties = new List<string>();
            return string.Join(Environment.NewLine, properties);
        }
    }
}
