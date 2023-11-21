using DbNetSuiteCore.Helpers;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using DbNetSuiteCore.Enums.DbNetFile;
using System.Linq;
using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.Components
{
    public class DbNetFileCore : DbNetSuiteCore
    {
        private readonly string _folder;
        public DbNetFileCore(string folder, string id = null) : base(id)
        {
            _folder = folder;
        }        
        /// <summary>
        /// Allows a user to do a wildcard search against file and folders names
        /// </summary>
        public bool? QuickSearch { get; set; }
        /// <summary>
        /// Controls the style of the toolbutton style
        /// </summary>
        public ToolbarButtonStyle? ToolbarButtonStyle { get; set; }
        /// <summary>
        /// Allows access to the search dialog for more comprehensive search capabilities
        /// </summary>
        public bool? Search { get; set; }
        /// <summary>
        /// Paging and navigation to the list of files and folders
        /// </summary>
        public bool? Navigation { get; set; }
        /// <summary>
        /// Export of the folder contents 
        /// </summary>
        public bool? Export { get; set; }
        /// <summary>
        /// Copies the folder contents to the clipboard
        /// </summary>
        public bool? Copy { get; set; }
        /// <summary>
        /// Upload a new file to the folder
        /// </summary>
        public bool? Upload { get; set; }
        /// <summary>
        /// Adds a caption to the top of the list of files and folders
        /// </summary>
        public string Caption { get; set; }
        public string Folder => _folder;
        /// <summary>
        /// Selects the columns to be displayed in the grid
        /// </summary>
        public List<FileInfoProperties> Columns { get; set; } = new List<FileInfoProperties>();
        /// <summary>
        /// Maximum height in pixels of the preview image
        /// </summary>
        public int? PreviewHeight { get; set; }
        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(EventType eventType, string functionName)
        {
            base.BindEvent(eventType, functionName);
        }
        /// <summary>
        /// Returns a reference to an instance of a given column for assigment of properties
        /// </summary>
        public DbNetFileCoreColumn Column(FileInfoProperties columnType)
        {
            return Column(new FileInfoProperties[] { columnType });
        }
        /// <summary>
        /// Returns a reference to an array of columns for assigment of properties
        /// </summary>
        public DbNetFileCoreColumn Column(FileInfoProperties[] columnTypes)
        {
            return new DbNetFileCoreColumn(columnTypes, _columnProperties);
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
{ColumnTypes()}
{ColumnProperties()}
{EventBindings()}
{Properties()}
{LinkedControls()}";
            return script;
        }

        private string Properties()
        {
            List<string> properties = new List<string>();
            AddProperty(Copy, nameof(Copy), properties);
            AddProperty(Export, $"{nameof(Export)}_", properties);
            AddProperty(Navigation, $"{nameof(Navigation)}", properties);
            AddProperty(QuickSearch, $"{nameof(QuickSearch)}", properties);
            AddProperty(Search, $"{nameof(Search)}", properties);
            AddProperty(Upload, $"{nameof(Upload)}", properties);
            AddProperty(Caption, $"{nameof(Caption)}", properties);
            AddProperty(ToolbarButtonStyle, $"{nameof(ToolbarButtonStyle)}", properties);
            AddProperty(PreviewHeight, $"{nameof(PreviewHeight)}", properties);
            properties.Add($"datePickerOptions = {DatePickerOptions()};");

            return string.Join(Environment.NewLine, properties);
        }

        private string ColumnTypes()
        {
            if (Columns.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnTypes(\"{string.Join("\",\"", Columns.Select(c => c.ToString()).ToList())}\");";
        }
    }
}
