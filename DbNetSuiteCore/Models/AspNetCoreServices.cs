using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace DbNetSuiteCore.Models
{


    public class AspNetCoreServices
    {
        private readonly HttpContext _httpContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
   
        public HttpContext httpContext => _httpContext;
        public IWebHostEnvironment webHostEnvironment => _webHostEnvironment;
        public IConfiguration configuration => _configuration;
        public IMemoryCache cache => _cache;

        public AspNetCoreServices(HttpContext httpContext, IWebHostEnvironment webHostEnvironment,IConfiguration configuration, IMemoryCache cache)
        {
            _httpContext = httpContext;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _cache = cache;
        }
    }
}
