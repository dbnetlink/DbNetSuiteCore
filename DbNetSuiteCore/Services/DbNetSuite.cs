﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using DbNetSuiteCore.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using DbNetSuiteCore.Helpers;
using System;
using System.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using DbNetSuiteCore.Utilities;
using System.Globalization;
using System.Threading;
using DbNetSuiteCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DbNetSuiteCore.Constants.DbNetSuite;

namespace DbNetSuiteCore.Services
{
    public class DbNetSuite
    {
        protected string Action => QueryParam("action");

        protected readonly HttpContext HttpContext;
        protected readonly IWebHostEnvironment Env;
        protected readonly IConfiguration Configuration;

        protected readonly DbNetSuiteCoreSettings Settings;

        private string _connectionString;

        public string ComponentId { get; set; } = String.Empty;
        public string ConnectionString
        {
            get => EncodingHelper.Decode(_connectionString);
            set => _connectionString = value;
        }
        public string Culture { get; set; } = String.Empty;
        protected DbNetDataCore Database { get; set; }

        public string Id => ComponentId;
        public ResourceManager ResourceManager { get; set; }

        public DbNetSuite(AspNetCoreServices services)
        {
            HttpContext = services.httpContext;
            Env = services.webHostEnvironment;
            Configuration = services.configuration;
            Settings = services.configuration.GetSection("DbNetSuiteCore").Get<DbNetSuiteCoreSettings>() ?? new DbNetSuiteCoreSettings();
        }

        public async Task<object> Process()
        {
            DbNetSuiteResponse response = new DbNetSuiteResponse();
            ResourceManager = new ResourceManager("DbNetSuiteCore.Resources.Localization.default", typeof(DbNetSuite).Assembly);

            switch (Action.ToLower())
            {
                case RequestAction.MessageBox:
                    await MessageBox(response);
                    break;
            }

            return SerialisedResponse(response);
        }
        protected string SerialisedResponse(DbNetSuiteResponse response)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return System.Text.Json.JsonSerializer.Serialize(response, serializeOptions);
        }
        protected string QueryParam(string name)
        {
            return HttpContext.Request.Query[name];
        }

        protected async Task MessageBox(DbNetSuiteResponse response)
        {
            var baseViewModel = new BaseViewModel();
            ReflectionHelper.CopyProperties(this, baseViewModel);
            response.Html = await HttpContext.RenderToStringAsync("Views/DbNetSuite/MessageBox.cshtml", baseViewModel);
        }

        protected void ThrowException(string Msg, string Info = null)
        {
        }

        protected async Task<string> GetResourceString(string resourceName)
        {
            byte[] buffer = await GetResource(resourceName);
            if (buffer == null)
            {
                throw new Exception($"Resource '{resourceName}' not found");
            }
            return new UTF8Encoding(false).GetString(buffer);
        }
        protected async Task<byte[]> GetResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream($"DbNetSuiteCore.Resources.{resourceName}"))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        byte[] buffer = new byte[stream.Length];
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                        return buffer;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        protected void Initialise()
        {
            Database = new DbNetDataCore(ConnectionString, Env, Configuration);
            ResourceManager = new ResourceManager("DbNetSuiteCore.Resources.Localization.default", typeof(DbNetSuite).Assembly);

            if (string.IsNullOrEmpty(Culture) == false)
            {
                CultureInfo ci = new CultureInfo(Culture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
        }

        protected async Task<T> DeserialiseRequest<T>()
        {
            var request = await GetRequest<T>();
            ReflectionHelper.CopyProperties(request, this);
            return request;
        }

        protected async Task<T> GetRequest<T>()
        {
            var request = HttpContext.Request;
            using (var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var json = await JObject.LoadAsync(jsonReader);
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                options.Converters.Add(new JsonStringEnumConverter());
                return json.ToObject<T>();
            }
        }

        public string Translate(string key)
        {
            return ResourceManager.GetString(key) ?? $"*{key}*";
        }
    }
}

