using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using DbNetSuiteCore.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using DbNetSuiteCore.Helpers;
using System.ComponentModel;
using System;
using System.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

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

        public string Id => ComponentId;
        public ResourceManager ResourceManager { get; set; }

        public DbNetSuite(AspNetCoreServices services)
        {
            HttpContext = services.httpContext;
            Env = services.webHostEnvironment;
            Configuration = services.configuration;
            Settings = services.configuration.GetSection("DbNetSuiteCore").Get<DbNetSuiteCoreSettings>() ?? new DbNetSuiteCoreSettings();
        }
        protected string QueryParam(string name)
        {
            return HttpContext.Request.Query[name];
        }

        protected void ThrowException(string Msg, string Info = null)
        {
        }

        protected async Task<string> GetResourceString(string resourceName)
        {
            byte[] buffer = await GetResource(resourceName);
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
    }
}

