using DbNetSuiteCore.Helpers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using DbNetSuiteCore.Enums.DbNetEdit;
using System;
using System.Linq;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Extensions;

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
        public ValidationMessageType? ValidationMessageType { get; set; }

        /// <summary>
        /// Creates a new instance of the form control
        /// </summary>
        /// <param name="connection">the name of the connection alias defined in appsettings.json</param>
        /// <param name="tableName">the table to be updated</param>
        /// <param name="id">the Id of the HTML element that is the container for the grid (optional)</param>
        /// <param name="databaseType">the type database to be connected to (optional)</param>
        public DbNetEditCore(string connection, string tableName, string id = null, DatabaseType? databaseType = null) : this(connection, tableName, id, databaseType, DataSourceType.TableOrView)
        {
        }
        /// <summary>
        /// Creates a new instance of the form control
        /// </summary>
        /// <param name="connection">the name of the connection alias defined in appsettings.json</param>
        /// <param name="databaseType">the type database to be connected to</param>
        /// <param name="tableName">the table to be updated</param>
        /// <param name="id">the Id of the HTML element that is the container for the grid (optional)</param>
        public DbNetEditCore(string connection, DatabaseType databaseType, string tableName, string id = null) : this(connection, tableName, id, databaseType)
        {
        }

        /// <summary>
        /// Creates a new instance of the grid control
        /// </summary>
        ///         
        /// <param name="dataSourceType">Should be JSON or List</param>
        /// <param name="url">the relative URL of the JSON file</param>
        /// <param name="jsonType">the type of a C# object that relates to the file</param>
        /// <param name="id">the Id of the HTML element that is the container for the grid</param>
        public DbNetEditCore(DataSourceType dataSourceType = DataSourceType.JSON, string url = null, Type jsonType = null, string id = null)
            : this(url ?? string.Empty, url?.Split('/').Last().ToLower().Replace(".json", string.Empty) ?? string.Empty, id, null, dataSourceType)
        {
            JsonType = jsonType;
        }

        internal DbNetEditCore(string connection, string tableName, string id = null, DatabaseType? databaseType = null, DataSourceType dataSourceType = DataSourceType.TableOrView) : base(connection, tableName, id, databaseType)
        {
            _dataSourceType = dataSourceType;

            switch (dataSourceType)
            {
                case DataSourceType.JSON:
                case DataSourceType.List:
                    BrowseControl = new DbNetGridCore(connection, tableName, true, dataSourceType, JsonType);
                    break;
                default:
                    BrowseControl = new DbNetGridCore(connection, tableName, true, databaseType);
                    break;
            }
            BrowseControl.Height = 600;
            this._browseDialogId = $"{this.Id}_browse_dialog";
        }


        internal DbNetEditCore(string connection, string tableName, bool editDialog, DatabaseType? databaseType = null, DataSourceType dataSourceType = DataSourceType.TableOrView) : base(connection, tableName, null, databaseType)
        {
            _dataSourceType = dataSourceType;
            IsEditDialog = editDialog;
        }

        internal DbNetEditCore(string connection, string tableName, bool editDialog, DataSourceType dataSourceType, Type jsonType) : base(connection, tableName, null, null)
        {
            _dataSourceType = dataSourceType;
            JsonType = jsonType;
            IsEditDialog = editDialog;
        }
        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(EventType eventType, string functionName)
        {
            base.BindEvent(eventType, functionName);
        }
        /// <summary>
        /// Browse control
        /// </summary>
        public DbNetGridCore BrowseControl { get; set; }
        /// <summary>
        /// Returns a reference to an instance of a named column for assigment of properties
        /// </summary>
        public DbNetEditCoreColumn Column(string columnName)
        {
            return Column(new string[] { columnName });
        }
        /// <summary>
        /// Returns a reference to an array of named columns for assigment of properties
        /// </summary>
        public DbNetEditCoreColumn Column(string[] columnNames)
        {
            return new DbNetEditCoreColumn(columnNames, _columnProperties, _fromPart, Columns);
        }
     
        public HtmlString Render()
        {
            if ((JsonKey ?? string.Empty) != _connection)
            {
                string message = ValidateProperties();

                if (string.IsNullOrEmpty(message) == false)
                {
                    return new HtmlString($"<div class=\"dbnetsuite-error\">{message}</div>");
                }
            }

            if (JsonType != null)
            {
                var propertyTypes = JsonType.PropertyTypes();
                foreach (string name in propertyTypes.Keys)
                {
                    Column(name).DataType(propertyTypes[name]);
                }
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
            AddProperty(ValidationMessageType, nameof(ValidationMessageType), properties);

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
