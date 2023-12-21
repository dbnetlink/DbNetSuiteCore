using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using DbNetLink.Middleware;
using Microsoft.Extensions.Configuration;
using AngleSharp;

namespace DbNetSuiteCore.Tests
{
    public class DbNetSuiteTests : IDisposable
    {
        protected readonly TestServer _server;
        protected readonly HttpClient _client;
        protected readonly IConfigurationRoot _configuration;

        public DbNetSuiteTests()
        {
            var builder = new WebHostBuilder()
                .UseWebRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(services =>
                {
                    services.AddDbNetSuiteCore();
                })
                .Configure(app =>
                {
                    app.UseDbNetSuiteCore();
                });
            
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var configPath2 = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile(configPath);
            });
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        public string? GetConnectionString(string connectionAlias)
        {
            return _configuration.GetConnectionString(connectionAlias)?.Replace("~", AppContext.BaseDirectory);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}
