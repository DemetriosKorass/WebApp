using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.DAL.Entities;
using Task = WebApp.DAL.Entities.Task;

namespace WebApp.DAL.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasOne(u => u.Role)
                   .WithMany() 
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasMany(u => u.Tasks)
                   .WithMany(t => t.Users)
                   .UsingEntity<Dictionary<string, object>>(
                       "UserTask",
                       j => j.HasOne<Task>().WithMany().HasForeignKey("TaskId"),
                       j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                       j =>
                       {
                           j.HasKey("UserId", "TaskId");
                           j.ToTable("UserTasks");
                       });
        }
    }
}
