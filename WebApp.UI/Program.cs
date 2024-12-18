using Microsoft.EntityFrameworkCore;
using WebApp.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

// Add MVC services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Use MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Index}/{id?}");

app.Run();
