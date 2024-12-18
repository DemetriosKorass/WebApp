using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebApp.DAL.Entities;
using Task = WebApp.DAL.Entities.Task;
namespace WebApp.DAL
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<Task> Tasks { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}