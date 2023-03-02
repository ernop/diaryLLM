using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenAI.GPT3.Managers;
using OpenAI.GPT3;

using static DiaryUI.Config;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouteDebugger();
app.UseDeveloperExceptionPage();

app.UseRouting();
app.UseDirectoryBrowser();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
    endpoints.MapControllerRoute(
        name: "Default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});



app.UseRouter(routes =>
{
    routes.MapGet("*", async context =>
    {
        await context.Response.WriteAsync("Sorry, couldn't find that!");
    });
});

app.Run();