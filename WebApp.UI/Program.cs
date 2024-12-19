using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using WebApp.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));


builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MathService>();

var app = builder.Build();

app.UseMiddleware<WebApp.UI.Middlewares.ExceptionHandlingMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Index}/{id?}");

app.Run();
