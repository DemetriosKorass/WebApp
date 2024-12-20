using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using WebApp.UI.Services;
using Microsoft.OpenApi.Models;
using NodaTime;
using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));


builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddSingleton<DateTimeService>();
builder.Services.AddSingleton<MathService>();

builder.Services.AddHangfire(x => x.UseMemoryStorage());
builder.Services.AddHangfireServer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API",
        Description = "An ASP.NET Core Web API for not problem",

    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

app.UseMiddleware<WebApp.UI.Middlewares.ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options => 
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = "swagger";
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Index}/{id?}");
app.UseHangfireDashboard();

app.Run();
