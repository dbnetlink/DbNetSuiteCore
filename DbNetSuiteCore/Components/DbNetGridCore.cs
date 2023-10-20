using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using DbNetSuiteCore.Enums.DbNetGrid;
using System.Linq;

namespace DbNetSuiteCore.Components
{
    public class DbNetGridCore : DbNetGridEditCore
    {
        internal string _editDialogId;
        internal bool? IsBrowseDialog { get; set; } = null;
        internal bool _hasEditDialog => this._linkedControls.Where(c => c is DbNetEditCore).Any(c => (c as DbNetEditCore).IsEditDialog ?? false == true);
        internal bool _addEditDialog => Edit && this._linkedControls.Where(c => c is DbNetEditCore).Any() == false;

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
        public Dictionary<string, object> ProcedureParams { get; set; } = new Dictionary<string, object>();
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

            EditControl = new DbNetEditCore(connection, fromPart, true);
            EditControl.IsEditDialog = true;

            this._editDialogId = $"{this.Id}_edit_dialog";
        }

        internal DbNetGridCore(string connection, string fromPart, bool browseControl) : base(connection, fromPart)
        {
            IsBrowseDialog = browseControl;
        }
       
        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(EventType eventType, string functionName)
        {
            base.BindEvent(eventType, functionName);
        }
        public DbNetGridCoreColumn Column(string columnName)
        {
            return Column(new string[] { columnName });
        }

        public DbNetGridCoreColumn Column(string[] columnNames)
        {
            return new DbNetGridCoreColumn(columnNames, _columnProperties, _fromPart, Columns);
        }
             
        public HtmlString Render()
        {
            string message = ValidateProperties();

            if (string.IsNullOrEmpty(message) == false)
            {
                return new HtmlString($"<div class=\"dbnetsuite-error\">{message}</div>");
            }

            AddEditDialogControl();

            if (NestedGrid != null)
            {
                NestedGrid.ParentChildRelationship = FromPart == NestedGrid.FromPart.ToLower() ? Enums.ParentChildRelationship.OneToOne : Enums.ParentChildRelationship.OneToMany;
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
            AddEditDialogControl();
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


        private void AddEditDialogControl()
        {
            if (_addEditDialog == false)
            {
                this._editDialogId = string.Empty;
                this.Update = false;
                this.Insert = false;
                EditControl = null;
            }
            else
            {
                if (EditControl.Columns.Any() == false)
                {
                    EditControl.Columns = new List<string>(this.Columns.ToArray());
                    EditControl.Labels = new List<string>(this.Labels.ToArray());
                    foreach (var columnProperty in this._columnProperties)
                    {
                        if (EditControl._columnProperties.Any(p => p.Equals(columnProperty)) == false)
                        {
                            EditControl._columnProperties.Add(columnProperty);
                        }
                    }
                }
                AddLinkedControl(EditControl);
            }
        }
        private string Properties()
        {
            List<string> properties = new List<string>();
            AddProperty(FrozenHeader, nameof(FrozenHeader), properties);
            AddProperty(PageSize, nameof(PageSize), properties);
            AddProperty(MultiRowSelect, nameof(MultiRowSelect), properties);
            AddProperty(BooleanDisplayMode, nameof(BooleanDisplayMode), properties);
            AddProperty(View, nameof(View), properties);
            AddProperty(AutoRowSelect, nameof(AutoRowSelect), properties);
            AddProperty(MultiRowSelectLocation, nameof(MultiRowSelectLocation), properties);
            AddProperty(GroupBy, nameof(GroupBy), properties);
            AddProperty(Copy, nameof(Copy), properties);
            AddProperty(Export, $"{nameof(Export)}_", properties);
            AddProperty(RowSelect, nameof(RowSelect), properties);
            AddProperty(EncodingHelper.Encode(ProcedureName), nameof(ProcedureName), properties);
            AddProperty(GridGenerationMode, nameof(GridGenerationMode), properties);
            AddProperty(Update, nameof(Update), properties);
            AddProperty(_editDialogId, "EditDialogId", properties);
            AddProperty(ViewLayoutColumns, nameof(ViewLayoutColumns), properties);
            AddProperty(IsBrowseDialog, nameof(IsBrowseDialog), properties);

            AddProperties(properties);

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
