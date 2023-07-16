using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using DbNetSuiteCore.Enums.DbNetGrid;
using DocumentFormat.OpenXml.Math;

namespace DbNetSuiteCore.Components
{
    public class DbNetGridCore : DbNetGridEditCore
    {
        private string DbNetEditId => this.Id.Replace("dbnetgrid", "dbnetedit");
        private string _editDialogId;
        /// <summary>
        /// Automatically selects the first row of the grid (default is true)
        /// </summary>
        public bool? AutoRowSelect { get; set; } = null;
        /// <summary>
        /// Controls the way a boolean value is displayed in the grid
        /// </summary> 
        public BooleanDisplayMode? BooleanDisplayMode { get; set; } = null;
        /// <summary>
        /// Adds/removes a page copy option to/from the toolbar
        /// </summary>
        public bool? Copy { get; set; } = null;
        /// <summary>
        /// Allow deletion of records
        /// </summary>
        public bool? Delete { get; set; } = null;
        public bool Edit => (Insert.HasValue && Insert.Value || Update.HasValue && Update.Value);
        /// <summary>
        /// Edit dialog control
        /// </summary>
        public DbNetEditCore EditControl { get; set; }
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
        /// Allow insertion of new records
        /// </summary>
        public bool? Insert { get; set; } = null;
        /// <summary>
        /// Enables selection of multiple rows
        /// </summary>
        public bool? MultiRowSelect { get; set; } = null;
        /// <summary>
        /// Controls the location of the multi-row select checkboxes
        /// </summary>
        public MultiRowSelectLocation? MultiRowSelectLocation { get; set; } = null;
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
        /// Highlights the selected row (default is true)
        /// </summary>
        public bool? RowSelect { get; set; } = null;
        /// <summary>
        /// Allow update of records
        /// </summary>
        public bool? Update { get; set; } = null;
        /// <summary>
        /// Adds/removes a view dialog option to the toolbar
        /// </summary>
        public bool? View { get; set; } = null;
        /// <summary>
        /// Specifies the number of columns in the View dialog layout 
        /// </summary>
        public int? ViewLayoutColumns { get; set; } = null;
        public DbNetGridCore(string connection, string fromPart, string id = null, DataSourceType dataSourceType = DataSourceType.TableOrView) : base(connection, fromPart, id)
        {
            if (dataSourceType == DataSourceType.StoredProcedure)
            {
                ProcedureName = fromPart;
            }

            EditControl = new DbNetEditCore(connection, fromPart, DbNetEditId);
            EditControl.IsEditDialog = true;

            this._linkedControls.Add(EditControl);

            this._editDialogId = $"{this.Id}_edit_dialog";
        }
      
        /// <summary>
        /// Assigns a grid column property to multiple columns
        /// </summary>
        public void SetColumnProperty(string[] columnNames, ColumnPropertyType propertyType, object propertyValue)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnProperty(columnName, propertyType, propertyValue);
            }
        }
        /// <summary>
        /// Assigns a grid column property value to a single column
        /// </summary>
        public void SetColumnProperty(string columnName, ColumnPropertyType propertyType, object propertyValue)
        {
            base.SetColumnProperty(columnName, propertyType, (object)propertyValue);
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
            if (Edit == false)
            {
                this._editDialogId = string.Empty;
            }

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
	{ConfigureLinkedControls()}
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
{ConfigureLinkedControls()}
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
{LinkedControls()}";
            return script;
        }

        private string Markup()
        {
            var gridMarkup = _idSupplied ? string.Empty : $"<section id=\"{_id}\"></section>";

            if (Edit)
            {
                gridMarkup += $"<div id=\"{this._editDialogId}\" class=\"edit-dialog\" title=\"Edit\" style=\"display:none\"><section id=\"{EditControl.Id}\"></section></div>";
            }

            return gridMarkup;
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
            AddProperty(Insert, nameof(Insert), properties);
            AddProperty(Update, nameof(Update), properties);
            AddProperty(Delete, nameof(Delete), properties);
            AddProperty(_editDialogId, "EditDialogId", properties);
            AddProperty(ViewLayoutColumns, nameof(ViewLayoutColumns), properties);

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

        private string ConfigureNestedGrid()
        {
            if (NestedGrid == null)
            {
                return string.Empty;
            }

            return NestedGrid.NestedRender();
        }
    }
}
