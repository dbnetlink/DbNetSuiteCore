using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Extensions;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DbNetSuiteCore.Components
{
    public class DbNetGridEditCore : DbNetSuiteCore
    {
        protected string _fromPart;
        protected bool? _insert = null;
        private string _jsonKey = null;
        internal DataSourceType _dataSourceType = DataSourceType.TableOrView;

        internal string FromPart => _fromPart;

        /// <summary>
        /// Selects the columns to be displayed in the grid
        /// </summary>
        public List<string> Columns { get; set; } = new List<string>();
        /// <summary>
        /// Allow deletion of records
        /// </summary>
        public bool? Delete { get; set; } = null;
        /// <summary>
        /// Use to assign values for any parameter placeholders used in the SQL
        /// </summary>
        public Dictionary<string, object> FixedFilterParams { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// Applies an SQL filter the grid 
        /// </summary>
        public string FixedFilterSql { get; set; } = null;
        /// <summary>
        /// Allow insertion of new records
        /// </summary>
        public bool? Insert
        {
            get => _insert; 
            set => _insert = value;
        }
        /// <summary>
        /// Labels for the columns specified in the Columns property
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();
        /// <summary>
        /// Maximum height in pixels for preview image in grid or edit panel
        /// </summary>
        public int? MaxImageHeight { get; set; } = null;
        /// <summary>
        /// Sets initial ordering of the records e.g. last_updated desc 
        /// </summary>
        public string InitialOrderBy { get; set; } = null;
        /// <summary>
        /// Optimizes the performance for large datasets
        /// </summary>
        public bool? OptimizeForLargeDataset { get; set; } = null;
        /// <summary>
        /// Displays a search box in the toolbar that allows for searching against all the text based columns
        /// </summary>
        public bool? QuickSearch { get; set; } = null;
        /// <summary>
        /// Adds/removes a page navigation to/from the toolbar
        /// </summary>
        public bool? Navigation { get; set; } = null;
        /// <summary>
        /// Adds/removes a search dialog option to/from the toolbar
        /// </summary>
        public bool? Search { get; set; } = null;
        /// <summary>
        /// Controls the style of the toolbar button
        /// </summary>
        public ToolbarButtonStyle? ToolbarButtonStyle { get; set; } = null;
        /// <summary>
        /// Controls the position of the toolbar
        /// </summary>
        public ToolbarPosition? ToolbarPosition { get; set; } = null;
        public Type JsonType { get; set; } = null;

        public DataSourceType DataSourceType
        {
            get => _dataSourceType;
            set => _dataSourceType = value;
        }
        internal string JsonKey
        {
            get => _jsonKey;
            set
            {
                _jsonKey = value;
                _connection = value;
            }
        }
        internal string Json { get; set; }
        /// <summary>
        /// Adds a generic list as a data source 
        /// </summary>
        public void AddList<T>(List<T> list, HttpContext httpContext)
        {
            ListToDataTable listToDataTable = new ListToDataTable();
            listToDataTable.AddList(list);
            JsonType = list.First().GetType();
            _fromPart = listToDataTable.DataTable.TableName;
            AssignJson(JsonConvert.SerializeObject(listToDataTable.DataTable), httpContext, true);
        }
        /// <summary>
        /// Adds a JSON string as a data source 
        /// </summary>
        public void AddJson<T>(string json, HttpContext httpContext = null)
        {
            JsonType = typeof(T);
            _fromPart = JsonType?.Name ?? "jsontable";
            AssignJson(json, httpContext);
        }

        public DbNetGridEditCore(string connection, string fromPart, string id = null, DatabaseType? databaseType = null) : base(connection, id, databaseType)
        {
            _fromPart = fromPart;
        }
    
        protected string ColumnExpressions()
        {
            if (Columns.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnExpressions(\"{string.Join("\",\"", Columns.Select(c => EncodingHelper.Encode(c)).ToList())}\");";
        }
        protected string ColumnKeys()
        {
            if (Columns.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnKeys(\"{string.Join("\",\"", Columns.Select(c => EncodingHelper.Encode(c.ToLower())).ToList())}\");";
        }
        protected string ColumnLabels()
        {
            if (Labels.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnLabels(\"{string.Join("\",\"", Labels)}\");";
        }

        protected void AddProperties(List<string> properties)
        {
            AddProperty(DataProvider, nameof(DataProvider), properties);
            AddProperty(ToolbarButtonStyle, nameof(ToolbarButtonStyle), properties);
            AddProperty(ToolbarPosition, nameof(ToolbarPosition), properties);
            AddProperty(Search, nameof(Search), properties);
            AddProperty(Culture, nameof(Culture), properties);
            AddProperty(QuickSearch, nameof(QuickSearch), properties);
            AddProperty(Navigation, nameof(Navigation), properties);
            AddProperty(Insert, nameof(Insert), properties);
            AddProperty(Delete, "_delete", properties);
            AddProperty(OptimizeForLargeDataset, nameof(OptimizeForLargeDataset), properties);
            AddProperty(ParentControlType, nameof(ParentControlType), properties);
            AddProperty(ParentChildRelationship, nameof(ParentChildRelationship), properties);
            AddProperty(MaxImageHeight, nameof(MaxImageHeight), properties);
            AddProperty(EncodingHelper.Encode(FixedFilterSql), nameof(FixedFilterSql), properties);
            AddProperty(EncodingHelper.Encode(InitialOrderBy), nameof(InitialOrderBy), properties);
            if (FixedFilterParams.Count > 0)
            {
                properties.Add($"fixedFilterParams = {Serialize(FixedFilterParams)};");
            }
            AddProperty(JsonKey, nameof(JsonKey), properties);
            AddProperty(Json, nameof(Json), properties, false);
            AddProperty((DataSourceType?)DataSourceType, nameof(DataSourceType), properties);

        }
        private void AssignJson(string json, HttpContext httpContext, bool isDataTable = false)
        {
            JsonKey = $"{(isDataTable ? "datatable" : string.Empty)}{Guid.NewGuid()}";
            if (httpContext != null)
            {
                try
                {
                    httpContext.Session.SetString(JsonKey, json);
                    return;
                }
                catch
                {
                }
            }
            Json = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json));

            if (this is DbNetGridCore)
            {
                var grid = (DbNetGridCore)this;
                if (grid.EditControl != null) {
                    grid.EditControl.AssignJson(json, httpContext, isDataTable);
                    grid.EditControl._fromPart = this._fromPart;
                }
            }

            if (this is DbNetEditCore)
            {
                var edit = (DbNetEditCore)this;
                if (edit.BrowseControl != null)
                {
                    edit.BrowseControl.AssignJson(json, httpContext, isDataTable);
                    edit.BrowseControl._fromPart = this._fromPart;
                }
            }
        }
    }
}