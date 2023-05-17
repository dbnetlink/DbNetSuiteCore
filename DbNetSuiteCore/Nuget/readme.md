
# DbNetSuiteCore

**DbNetSuiteCore** is a .Net Core middleware Nuget package that can be used to add database driven web-reporting to any ASP.NET Core web application in minutes with just a few lines of code.

```c#
{
    using DbNetLink.Middleware;           // <= Add this line

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    builder.Services.AddRazorPages();
    builder.Services.AddDbNetSuiteCore(); // <= Add this line
    
    WebApplication app = builder.Build();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();
    app.UseDbNetSuiteCore();              // <= Add this line
       
    app.UseEndpoints(endpoints =>
    {
	    endpoints.MapRazorPages();
    });

    app.Run();
}
```
   
The solution comprises of the **DbNetSuiteCore** library and a sample application running against an SQLite database

For more information and demos [click here](https://dbnetsuitecoreapp.azurewebsites.net/).