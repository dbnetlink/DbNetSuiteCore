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
        ///         
        public string FromPart => _fromPart;

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
        /// <summary>
        /// Creates a new instance of the combo control
        /// </summary>
        /// <param name="connection">the name of the connection alias defined in appsettings.json</param>
        /// <param name="fromPart">table, view, sql or stored procedure name</param>
        /// <param name="valueColumn">the name of the column that contains the value</param>
        /// <param name="textColumn">the name of the column that contains the description</param>
        /// <param name="id">the Id of the HTML element that is the container for the grid</param>
        /// <param name="dataSourceType">the type of data source. Specify StoredProcedure if fromPart is a stored procedure name</param>
        /// <param name="databaseType">the type database to be connected to (optional)</param>
        public DbNetComboCore(string connection, string fromPart, string valueColumn, string textColumn = null, string id = null, DataSourceType dataSourceType = DataSourceType.TableOrView, DatabaseType? databaseType = null) : base(connection, id, databaseType)
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
        /// Creates a new instance of the grid control
        /// </summary>
        /// <param name="connection">the name of the connection alias defined in appsettings.json</param>
        /// <param name="databaseType">the type database to be connected to (optional)</param>
        /// <param name="valueColumn">the name of the column that contains the value</param>
        /// <param name="textColumn">the name of the column that contains the description</param>
        /// <param name="id">the Id of the HTML element that is the container for the grid</param>
        /// <param name="dataSourceType">the type of data source. Specify StoredProcedure if fromPart is a stored procedure name</param>
        public DbNetComboCore(string connection, DatabaseType databaseType, string fromPart, string valueColumn, string textColumn = null, string id = null, DataSourceType dataSourceType = DataSourceType.TableOrView) : this(connection, fromPart, valueColumn, textColumn, id, dataSourceType, databaseType)
        {
        }

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

        private string Properties()
        {
            List<string> properties = new List<string>();
            AddProperty(DataProvider, nameof(DataProvider), properties);
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
    
    }
}
