using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;
using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DbNetSuiteCore.Components
{
    public class DbNetSuiteCore
    {
        protected readonly string _id;
        protected readonly string _connection;
        protected DbNetSuiteCoreSettings _DbNetSuiteCoreSettings;
        protected readonly IConfigurationRoot _configuration;
        protected readonly bool _idSupplied = false;
        protected List<DbNetSuiteCore> _linkedControls { get; set; } = new List<DbNetSuiteCore>();

        private List<EventBinding> _eventBindings { get; set; } = new List<EventBinding>();
        protected string ComponentTypeName => this.GetType().Name.Replace("Core", string.Empty);
        /// <summary>
        /// Overrides the default culture that controls default date and currency formatting
        /// </summary>
        public string Culture { get; set; } = null;
        public string Id => _id;
        /// <summary>
        /// Specifies the type of provider for the database connection
        /// </summary>
        public DataProvider? DataProvider { get; set; } = null;

        public DbNetSuiteCore(string connection, string id = null)
        {
            _idSupplied = id != null;
            _id = id ?? $"{ComponentTypeName.ToLower()}{Guid.NewGuid().ToString().Split("-").First()}";
            _connection = connection;
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
            _linkedControls.Add(linkedControl);
        }
        /// <summary>
        /// Binds an event to a named client-side JavaScript function
        /// </summary>
        public void Bind(Enum eventType, string functionName)
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

        protected string ValidateProperties()
        {
            string message = string.Empty;

            string connectionString = _configuration.GetConnectionString(_connection);

            if (connectionString == null)
            {
                message = $"Connection string [{_connection}] not found. Please check the connection strings in your appsettings.json file";
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

        protected void AddProperty(string property, string name, List<string> properties)
        {
            if (string.IsNullOrEmpty(property) == false)
            {
                properties.Add($"{LowerCaseFirstLetter(name)} = \"{property}\";");
            };
        }

        protected string InitScript()
        {
            return @$" 
document.addEventListener('DOMContentLoaded', function() {{init_{_id}()}});
function init_{_id}()
{{
    if (typeof({ComponentTypeName}) == 'undefined') {{alert('DbNetSuite client-side code has not loaded. Add @DbNetSuiteCore.ClientScript() to your razor page. See console for details');console.error(""DbNetSuite stylesheet not found. See https://dbnetsuitecore.z35.web.core.windows.net/index.htm?context=20#DbNetSuiteCoreClientScript"");return;}};";
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

            if (this is DbNetEditCore)
            {
                DbNetEditCore dbNetEditCore = (DbNetEditCore)this;
                if (dbNetEditCore._browse)
                {
                    markup += $"<div id=\"{dbNetEditCore._browseDialogId}\" class=\"browse-dialog\" title=\"Browse\" style=\"display:none\"><section id=\"{dbNetEditCore.BrowseControl.Id}\"></section></div>";
                }
            }

            if (this is DbNetGridCore)
            {
                DbNetGridCore dbNetGridCore = (DbNetGridCore)this;
                if (dbNetGridCore.Edit)
                {
                    markup += $"<div id=\"{dbNetGridCore._editDialogId}\" class=\"edit-dialog\" title=\"Edit\" style=\"display:none\"><section id=\"{dbNetGridCore.EditControl.Id}\"></section></div>";
                }
            }

            foreach (var linkedControl in _linkedControls)
            {
                markup += linkedControl.Markup();
            }

            return markup;
        }
    }
}


