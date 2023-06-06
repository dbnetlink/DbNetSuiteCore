using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using DbNetLink.Middleware;
using Microsoft.Extensions.Configuration;

namespace DbNetSuiteCore.Tests
{
    public class DbNetSuiteTests : IDisposable
    {
        protected readonly TestServer _server;
        protected readonly HttpClient _client;

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

            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile(configPath);
            });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}
