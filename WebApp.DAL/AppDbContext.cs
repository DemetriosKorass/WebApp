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

            var defaultRole = new Role { Id = 1, Name = "Default User", Permissions = Permissions.Read };
            var adminRole = new Role { Id = 2, Name = "Admin", Permissions = Permissions.Read | Permissions.Write | Permissions.Delete };
            
            modelBuilder.Entity<Role>().HasData(
                defaultRole,
                adminRole
            );
            modelBuilder.Entity<Task>().HasData(
                new Task { Id = 1, Name = "Work"},
                new Task { Id = 2, Name = "Eat"},
                new Task { Id = 3, Name = "Sleep" }
            );
        }

    }
}