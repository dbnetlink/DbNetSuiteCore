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
using Azure.Messaging;
using System.Configuration;
using System.Data;

namespace DbNetSuiteCore.Components
{
    public class DbNetGridCore : DbNetSuiteCore
    {
        private readonly string _fromPart;
        private List<ColumnProperty> _columnProperties { get; set; } = new List<ColumnProperty>();
        private List<DbNetGridCore> _linkedGrids { get; set; } = new List<DbNetGridCore>();
        /// <summary>
        /// Automatically selects the first row of the grid (default is true)
        /// </summary>
        public bool? AutoRowSelect { get; set; } = null;
        /// <summary>
        /// Controls the way a boolean value is displayed in the grid
        /// </summary> 
        public BooleanDisplayMode? BooleanDisplayMode { get; set; } = null;
        /// <summary>
        /// Selects the columns to be displayed in the grid
        /// </summary>
        public List<string> Columns { get; set; } = new List<string>();
        /// <summary>
        /// Adds/removes a page copy option to/from the toolbar
        /// </summary>
        public bool? Copy { get; set; } = null;
        /// <summary>
        /// Adds/removes a grid export option to/from the toolbar
        /// </summary>
        public bool? Export { get; set; } = null;
        /// <summary>
        /// Use to assign values for any parameter placeholders used in the SQL
        /// </summary>
        public Dictionary<string, object> FixedFilterParams { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// Applies an SQL filter the the grid 
        /// </summary>
        public string FixedFilterSql { get; set; } = null;
        /// <summary>
        /// When set to true will prevent the grid headings from scrolling off the page
        /// </summary>
        public bool? FrozenHeader { get; set; } = null;
        /// <summary>
        /// Configures linked Google chart
        /// </summary>
        public GoogleChartOptions GoogleChartOptions { get; set; } = null;
        /// <summary>
        /// Set to DataTable to interface with 3rd party client-side rendering tool
        /// </summary>
        public GridGenerationMode? GridGenerationMode { get; set; }
        /// <summary>
        /// Groups the returned data by all the columns not identified as aggregates
        /// </summary>
        public bool? GroupBy { get; set; } = null; 
        /// <summary>
        /// Labels for the columns specified in the Columns property
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();
        /// <summary>
        /// Enables selection of multiple rows
        /// </summary>
        public bool? MultiRowSelect { get; set; } = null;
        /// <summary>
        /// Controls the location of the multi-row select checkboxes
        /// </summary>
        public MultiRowSelectLocation? MultiRowSelectLocation { get; set; } = null;
        /// <summary>
        /// Adds/removes a page navigation to/from the toolbar
        /// </summary>
        public bool? Navigation { get; set; } = null;
        /// <summary>
        /// The grid component that is nested inside this grid
        /// </summary>
        public DbNetGridCore NestedGrid { get; set; } = null;
        /// <summary>
        /// Optimizes the grid performance when used against very large datasets
        /// </summary>
        public bool? OptimizeForLargeDataset { get; set; } = null;
        /// <summary>
        /// Number of rows displayed per page
        /// </summary>
        public int? PageSize { get; set; } = null;
        /// <summary>
        /// The name of the stored procedure used as the data source
        /// </summary>
        public string ProcedureName { get; set; } = null;
        /// <summary>
        /// The parameters passed to the stored procedure
        /// </summary>
        public Dictionary<string, object> ProcedureParams { get; set; } =  new Dictionary<string, object>();
        /// <summary>
        /// Displays a search box in the toolbar that allows for searching against all the text based columns
        /// </summary>
        public bool? QuickSearch { get; set; } = null;
        /// <summary>
        /// Highlights the selected row (default is true)
        /// </summary>
        public bool? RowSelect { get; set; } = null;
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
        /// <summary>
        /// Adds/removes a view dialog option to the toolbar
        /// </summary>
        public bool? View { get; set; } = null;

        public DbNetGridCore(string connection, string fromPart, string id = null, DataSourceType dataSourceType = DataSourceType.TableOrView) : base(connection, id)
        {
            _fromPart = fromPart;
            if (dataSourceType == DataSourceType.StoredProcedure)
            {
                ProcedureName = fromPart;
            }
        }
      
        /// <summary>
        /// Assigns a property value to multiple columns
        /// </summary>
        public void SetColumnProperty(string[] columnNames, ColumnPropertyType propertyType, object propertyValue)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnProperty(columnName, propertyType, propertyValue);
            }
        }
        /// <summary>
        /// Assigns a property value to a single column
        /// </summary>
        public void SetColumnProperty(string columnName, ColumnPropertyType propertyType, object propertyValue)
        {
            columnName = FindColumnName(columnName);
            _columnProperties.Add(new ColumnProperty(columnName.ToLower(), propertyType, propertyValue));

            string FindColumnName(string name)
            {
                if (Columns.Any(c => c.ToLower() == name.ToLower()))
                {
                    return name;
                }

                var splitChars = new string[] { ".", " " };
                foreach (string splitChar in splitChars)
                {
                    var columnExpr = Columns.FirstOrDefault(c => c.Split(splitChar).Last().ToLower() == name.ToLower());

                    if (columnExpr != null)
                    {
                        return columnExpr;
                    }
                }

                if (propertyType == ColumnPropertyType.DataOnly)
                {
                    Columns.Add(name);
                }
                return name;
            }
        }

        /// <summary>
        /// Links one grid component to another
        /// </summary>
        public void AddLinkedGrid(DbNetGridCore linkedGrid)
        {
            _linkedGrids.Add(linkedGrid);
        }

        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(DbNetGridEventType eventType, string functionName)
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
            if (GoogleChartOptions != null)
            {
                script = $"<script type=\"text/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>{Environment.NewLine}";
            }
            script += @$" 
                {Markup()}
                <script type=""text/javascript"">
                {ClientJavaScript()}
                </script>";

            return new HtmlString(script);
        }

        private string ClientJavaScript()
        {
            var script = @$"{InitScript()}
	{ConfigureLinkedGrids()}
	var {_id} = new DbNetGrid('{_id}');
	with ({_id})
	{{
		{GridConfiguration()}                            
		initialize();
	}}
}}
{ConfigureNestedGrid()}";

            if (GetCurrentSettings().Debug == false)
            {
                script = script.Replace("\r\n", string.Empty).Replace("\t", string.Empty);
            }
            return script;
        }

        private string NestedRender()
        {
            var script = @$" 
function configure{_id}(grid)
{{
	with (grid)
	{{
		{GridConfiguration()}                        
	}}
}}
{ConfigureNestedGrid()}";

            return script;
        }

        internal string LinkedRender()
        {
            var script = @$" 
{ConfigureLinkedGrids()}
var {_id} = new DbNetGrid('{_id}');
with ({_id})
{{
	{GridConfiguration()}                        
}}";

            return script;
        }

        private string GridConfiguration()
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
{LinkedGrids()}";
            return script;
        }

        private string Markup()
        {
            return _idSupplied ? string.Empty : $"<section id=\"{_id}\"></section>";
        }
        private string Properties()
        {
            List<string> properties = new List<string>();
            AddProperty(FrozenHeader, nameof(FrozenHeader), properties);
            AddProperty(PageSize, nameof(PageSize), properties);
            AddProperty(ToolbarButtonStyle, nameof(ToolbarButtonStyle), properties);
            AddProperty(ToolbarPosition, nameof(ToolbarPosition), properties);
            AddProperty(MultiRowSelect, nameof(MultiRowSelect), properties);
            AddProperty(QuickSearch, nameof(QuickSearch), properties);
            AddProperty(BooleanDisplayMode, nameof(BooleanDisplayMode), properties);
            AddProperty(View, nameof(View), properties);
            AddProperty(OptimizeForLargeDataset, nameof(OptimizeForLargeDataset), properties);
            AddProperty(AutoRowSelect, nameof(AutoRowSelect), properties);
            AddProperty(MultiRowSelectLocation, nameof(MultiRowSelectLocation), properties);
            AddProperty(GroupBy, nameof(GroupBy), properties);
            AddProperty(Copy, nameof(Copy), properties);
            AddProperty(Search, nameof(Search), properties);
            AddProperty(Export, $"{nameof(Export)}_", properties);
            AddProperty(Culture, nameof(Culture), properties);
            AddProperty(EncodingHelper.Encode(FixedFilterSql), nameof(FixedFilterSql), properties);
            AddProperty(RowSelect, nameof(RowSelect), properties);
            AddProperty(EncodingHelper.Encode(ProcedureName), nameof(ProcedureName), properties);
            AddProperty(GridGenerationMode, nameof(GridGenerationMode), properties);
            AddProperty(Navigation, nameof(Navigation), properties);

            if (FixedFilterParams.Count > 0)
            {
                properties.Add($"fixedFilterParams = {Serialize(FixedFilterParams)};");
            }

            if (ProcedureParams?.Count > 0)
            {
                properties.Add($"procedureParams = {Serialize(ProcedureParams)};");
            }

            properties.Add($"datePickerOptions = {DatePickerOptions()};");

            if (GoogleChartOptions != null)
            {
                properties.Add($"googleChartOptions = {Serialize(GoogleChartOptions)};");
            }

            if (NestedGrid != null)
            {
                properties.Add($"addNestedGrid(configure{NestedGrid.Id});");
            }
            return string.Join(Environment.NewLine, properties);
        }

        private string ColumnExpressions()
        {
            if (Columns.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnExpressions(\"{string.Join("\",\"", Columns.Select(c => EncodingHelper.Encode(c)).ToList())}\");";
        }
        private string ColumnKeys()
        {
            if (Columns.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnKeys(\"{string.Join("\",\"", Columns.Select(c => EncodingHelper.Encode(c.ToLower())).ToList())}\");";
        }
        private string ColumnLabels()
        {
            if (Labels.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnLabels(\"{string.Join("\",\"", Labels)}\");";
        }

        private string ColumnProperties()
        {
            var script = _columnProperties.Select(x => $"setColumnProperty(\"{EncodingHelper.Encode(x.ColumnName)}\",\"{LowerCaseFirstLetter(x.PropertyType.ToString())}\",{PropertyValue(x.PropertyValue, x.PropertyType)});").ToList();
            return string.Join(Environment.NewLine, script);

            string PropertyValue(object value, ColumnPropertyType propertyType)
            {
                if (value is bool)
                {
                    return value.ToString()!.ToLower();
                }
                if (propertyType == ColumnPropertyType.Lookup)
                {
                    value = EncodingHelper.Encode(value.ToString());
                }
                return $"\"{value}\"";
            }
        }

        private string LinkedGrids()
        {
            var script = new List<string>();
            script = _linkedGrids.Select(x => $"addLinkedGrid({x.Id});").ToList();
            return string.Join(Environment.NewLine, script);
        }

        private string ConfigureNestedGrid()
        {
            if (NestedGrid == null)
            {
                return string.Empty;
            }

            return NestedGrid.NestedRender();
        }

        private string DatePickerOptions()
        {
            DatePickerOptions datePickerOptions = new DatePickerOptions(this.Culture);
            return Serialize(datePickerOptions);
        }

        private string ConfigureLinkedGrids()
        {
            var script = string.Empty;

            foreach (var linkedGrid in _linkedGrids)
            {
                script += linkedGrid.LinkedRender();
            }

            return script;
        }
    }
}
