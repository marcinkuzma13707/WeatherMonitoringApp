using Microsoft.EntityFrameworkCore;
using WeatherMonitoringApp.Configuration;
using WeatherMonitoringApp.Data;
using WeatherMonitoringApp.Middleware;
using WeatherMonitoringApp.Repositories;
using WeatherMonitoringApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EnvironmentSettings>(builder.Configuration.GetSection("EnvironmentSettings"));

builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<WeatherService>();

builder.Services.AddHostedService<WeatherBackgroundService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";

        if (app.Environment.IsDevelopment())
        {
            spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
        }
    });
});

app.Run();