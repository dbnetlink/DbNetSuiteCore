using DbNetLink.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbNetSuiteCore();

/*
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5); // Set session timeout
});
*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseDbNetSuiteCore();
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();


#pragma warning disable CA1050 // Declare types in namespaces
public partial class Program
{
}
#pragma warning restore CA1050 // Declare types in namespaces
