﻿using DbNetSuiteCore.Enums;
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
        private string ComponentTypeName => this.GetType().Name.Replace("Core", string.Empty);

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
    }
}


