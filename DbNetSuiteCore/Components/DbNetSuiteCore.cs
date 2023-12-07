using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web;

namespace DbNetSuiteCore.Components
{
    public class DbNetSuiteCore
    {
        protected readonly string _id;
        protected string _connection;
        protected DbNetSuiteCoreSettings _DbNetSuiteCoreSettings;
        protected readonly IConfigurationRoot _configuration;
        protected readonly bool _idSupplied = false;
        internal bool _isChildControl = false;
        internal List<ColumnProperty> _columnProperties { get; set; } = new List<ColumnProperty>();

        protected List<DbNetSuiteCore> _linkedControls { get; set; } = new List<DbNetSuiteCore>();

        private List<EventBinding> _eventBindings { get; set; } = new List<EventBinding>();
        protected string ComponentTypeName => this.GetType().Name.Replace("Core", string.Empty);
        /// <summary>
        /// Overrides the default culture that controls default date and currency formatting
        /// </summary>
        public string Culture { get; set; } = null;
        public string Id => _id;
        internal DataProvider? DataProvider => GetDataProvider();
        /// <summary>
        /// Specifies the type of database to be connected to
        /// </summary>
        public DatabaseType? DatabaseType { get; set; } = null;
        public string ParentControlType { get; set; } = null;
        public ParentChildRelationship? ParentChildRelationship { get; set; } = null;

        public DbNetSuiteCore(string connection, string id = null, DatabaseType? databaseType = null)
        {
            _idSupplied = id != null;
            _id = id ?? $"{ComponentTypeName.ToLower()}{Guid.NewGuid().ToString().Split("-").First()}";
            _connection = connection;
            _configuration = LoadConfiguration();
            DatabaseType = databaseType;
        }

        public DbNetSuiteCore(string id = null)
        {
            _id = id ?? $"{ComponentTypeName.ToLower()}{Guid.NewGuid().ToString().Split("-").First()}";
            _configuration = LoadConfiguration();
        }
        public static HtmlString StyleSheet(string fontSize = null, string fontFamily = null)
        {
            List<string> url = new List<string>() { "~/resource.dbnetsuite?action=css" };
            if (string.IsNullOrEmpty(fontSize) == false)
            {
                url.Add($"font-size={fontSize}");
            }
            if (string.IsNullOrEmpty(fontFamily) == false)
            {
                url.Add($"font-family={fontFamily}");
            }

            return new HtmlString($"<link href=\"{string.Join("&", url)}\" type=\"text/css\" rel=\"stylesheet\" />");
        }

        public static HtmlString ClientScript()
        {
            return new HtmlString($"<script src=\"~/resource.dbnetsuite?action=script\"></script>");
        }

        /// <summary>
        /// Links one DbNetSuite component to another
        /// </summary>
        public void AddLinkedControl(DbNetSuiteCore linkedControl)
        {
            linkedControl.ParentControlType = this.GetType().Name.Replace("Core", string.Empty);
            linkedControl._isChildControl = true;
            linkedControl.DatabaseType = this.DatabaseType;

            if (linkedControl.ParentChildRelationship.HasValue == false)
            {
                if ((this is DbNetGridEditCore || this is DbNetComboCore) && linkedControl is DbNetGridEditCore)
                {
                    string fromPart;
                    if (this is DbNetGridEditCore)
                    {
                        fromPart = (this as DbNetGridEditCore).FromPart.ToLower();
                    }
                    else
                    {
                        fromPart = (this as DbNetComboCore).FromPart.ToLower();
                    }
                    linkedControl.ParentChildRelationship = fromPart == (linkedControl as DbNetGridEditCore).FromPart.ToLower() ? Enums.ParentChildRelationship.OneToOne : Enums.ParentChildRelationship.OneToMany;
                    /*
                                        if (this is DbNetComboCore && linkedControl is DbNetEditCore && linkedControl.ParentChildRelationship == Enums.ParentChildRelationship.OneToOne)
                                        {
                                            (linkedControl as DbNetEditCore).Navigation = false;
                                        }
                    */
                }
            }
            _linkedControls.Add(linkedControl);
        }
        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        protected void BindEvent(Enum eventType, string functionName)
        {
            _eventBindings.Add(new EventBinding(eventType, functionName));
        }
        private IConfigurationRoot LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();
            return configuration;
        }

        protected DbNetSuiteCoreSettings GetCurrentSettings()
        {
            if (_DbNetSuiteCoreSettings == null)
            {
                _DbNetSuiteCoreSettings = _configuration.GetSection("DbNetSuiteCore").Get<DbNetSuiteCoreSettings>() ?? new DbNetSuiteCoreSettings();
            }

            return _DbNetSuiteCoreSettings;
        }
        protected string ColumnProperties()
        {
            var script = _columnProperties.Select(x => $"setColumnProperty(\"{EncodeColumnName(x.ColumnName)}\",\"{LowerCaseFirstLetter(x.PropertyType.ToString())}\",{PropertyValue(x.PropertyValue, x.PropertyType)});").ToList();
            return string.Join(Environment.NewLine, script);

            string PropertyValue(object value, Enum propertyType)
            {
                if (value is bool)
                {
                    return value.ToString()!.ToLower();
                }

                switch (propertyType)
                {
                    case ColumnPropertyType.Lookup:
                        value = EncodingHelper.Encode(value.ToString());
                        break;
                    case ColumnPropertyType.Annotation:
                        value = HttpUtility.HtmlEncode(value.ToString());
                        break;
                    case ColumnPropertyType.InputValidation:
                        return Serialize(value);
                }

                return $"\"{value}\"";
            }
        }

        private string EncodeColumnName(string columnName)
        {
            return this is DbNetFileCore ? columnName : EncodingHelper.Encode(columnName);
        }

        protected string ValidateProperties()
        {
            string message = string.Empty;

            if ((this is DbNetFileCore) == false)
            {
                if (_connection.ToLower().EndsWith(".json") == false)
                {
                    string connectionString = _configuration.GetConnectionString(_connection);

                    if (connectionString == null)
                    {
                        message = $"Connection string [{_connection}] not found. Please check the connection strings in your appsettings.json file";
                    }
                }
            }

            if (_isChildControl)
            {
                message = $"The Render() method should not be called on a child control. This will be invoked automatically by the parent control.";
            }
            return message;
        }

        protected string Serialize(object obj)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return JsonSerializer.Serialize(obj, options);
        }

        protected string EventBindings()
        {
            var script = new List<string>();
            script = _eventBindings.Select(x => $"bind(\"{LowerCaseFirstLetter(x.EventType.ToString())}\",{x.FunctionName});").ToList();
            return string.Join(Environment.NewLine, script);
        }

        protected string LowerCaseFirstLetter(string str)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        protected void AddProperty<T>(T? property, string name, List<string> properties) where T : struct
        {
            if (property.HasValue/* && !property.Value.Equals(default(T))*/)
            {
                properties.Add($"{LowerCaseFirstLetter(name)} = {PropertyValue(property)};");
            };

            string PropertyValue(object value)
            {
                if (typeof(T).IsEnum)
                {
                    return $"\"{value}\"";
                }

                return value.ToString().ToLower();
            }
        }

        protected void AddProperty(string property, string name, List<string> properties, bool quoted = true)
        {
            if (string.IsNullOrEmpty(property) == false)
            {
                string quote = quoted ? "\"" : string.Empty;
                properties.Add($"{LowerCaseFirstLetter(name)} = {quote}{property}{quote};");
            };
        }

        protected string InitScript()
        {
            return @$" 
document.addEventListener('DOMContentLoaded', function() {{init_{_id}()}});
function init_{_id}()
{{
    if (typeof({ComponentTypeName}) == 'undefined') {{alert('DbNetSuite client-side code has not loaded. Add @DbNetSuiteCore.ClientScript() to your razor page. See console for details');console.error(""DbNetSuite stylesheet not found. See https://docs.dbnetsuitecore.com/index.htm?context=20#DbNetSuiteCoreClientScript"");return;}};";
        }

        protected string LinkedControls()
        {
            var script = new List<string>();
            script = _linkedControls.Select(x => $"addLinkedControl({x.Id});").ToList();
            return string.Join(Environment.NewLine, script);
        }
        protected string ConfigureLinkedControls()
        {
            var script = string.Empty;

            foreach (var linkedControl in _linkedControls)
            {
                if (linkedControl is DbNetGridCore)
                {
                    script += (linkedControl as DbNetGridCore).LinkedRender();
                }
                if (linkedControl is DbNetComboCore)
                {
                    script += (linkedControl as DbNetComboCore).LinkedRender();
                }

                if (linkedControl is DbNetEditCore)
                {
                    script += (linkedControl as DbNetEditCore).LinkedRender();
                }

                if (linkedControl is DbNetFileCore)
                {
                    script += (linkedControl as DbNetFileCore).LinkedRender();
                }
            }

            return script;
        }
        protected string DatePickerOptions()
        {
            DatePickerOptions datePickerOptions = new DatePickerOptions(this.Culture);
            return Serialize(datePickerOptions);
        }

        internal string Markup()
        {
            string markup = _idSupplied ? string.Empty : $"<section id=\"{_id}\"></section>";

            if (HasBrowseDialog(this))
            {
                markup += BrowseDialogMarkup((DbNetEditCore)this);
            }

            if (HasEditDialog(this))
            {
                markup += EditDialogMarkup((DbNetGridCore)this);
            }

            if (HasSearchResultsDialog(this))
            {
                markup += SearchResultsDialogMarkup((DbNetFileCore)this);
            }

            foreach (var linkedControl in _linkedControls)
            {
                if (IsEditDialog(linkedControl) || IsBrowseDialog(linkedControl))
                {
                    continue;
                }
                markup += linkedControl.Markup();
            }

            return markup;
        }

        private string BrowseDialogMarkup(DbNetEditCore dbNetEditCore)
        {
            return $"<div id=\"{dbNetEditCore._browseDialogId}\" class=\"browse-dialog\" title=\"Browse\" style=\"display:none\"><section id=\"{dbNetEditCore.BrowseControl.Id}\"></section></div>";
        }

        private string EditDialogMarkup(DbNetGridCore dbNetGridCore)
        {
            return $"<div id=\"{dbNetGridCore._editDialogId}\" class=\"edit-dialog\" title=\"Edit\" style=\"display:none\"><section id=\"{dbNetGridCore.EditControl.Id}\"></section></div>"; ;
        }

        private string SearchResultsDialogMarkup(DbNetFileCore dbNetFileCore)
        {
            return $"<div id=\"{dbNetFileCore._searchResultsDialogId}\" class=\"search-results-dialog\" title=\"Search Results\" style=\"display:none\"><section id=\"{dbNetFileCore.SearchResultsControl.Id}\"></section></div>";
        }

        private bool IsEditDialog(DbNetSuiteCore control)
        {
            if (control is DbNetEditCore)
            {
                DbNetEditCore dbNetEditCore = (DbNetEditCore)control;
                return dbNetEditCore.IsEditDialog ?? false;
            }
            return false;
        }

        private bool IsBrowseDialog(DbNetSuiteCore control)
        {
            if (control is DbNetGridCore)
            {
                DbNetGridCore dbNetGridCore = (DbNetGridCore)control;
                return dbNetGridCore.IsBrowseDialog ?? false;
            }
            return false;
        }

        private bool HasEditDialog(DbNetSuiteCore control)
        {
            if (control is DbNetGridCore)
            {
                DbNetGridCore dbNetGridCore = (DbNetGridCore)control;
                return dbNetGridCore._hasEditDialog || dbNetGridCore._addEditDialog;
            }
            return false;
        }

        private bool HasBrowseDialog(DbNetSuiteCore control)
        {
            if (control is DbNetEditCore)
            {
                DbNetEditCore dbNetEditCore = (DbNetEditCore)control;
                return dbNetEditCore._browse;
            }
            return false;
        }

        private bool HasSearchResultsDialog(DbNetSuiteCore control)
        {
            if (control is DbNetFileCore)
            {
                DbNetFileCore dbNetFileCore = (DbNetFileCore)control;
                return dbNetFileCore._searchResultsDialogId != null;
            }
            return false;
        }

        protected string GetMimeTypeForFileExtension(string extension)
        {
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType($".{extension}", out string contentType))
            {
                contentType = string.Empty;
            }

            return contentType;
        }

        private DataProvider? GetDataProvider()
        {
            if (DatabaseType.HasValue)
            {
                switch (DatabaseType.Value)
                {
                    case Enums.DatabaseType.PostgreSQL:
                        return Enums.DataProvider.Npgsql;
                    case Enums.DatabaseType.MySQL:
                        return Enums.DataProvider.MySql;
                    case Enums.DatabaseType.MariaDB:
                        return Enums.DataProvider.MySqlConnector;
                }
            }

            return null;
        }
    }
}