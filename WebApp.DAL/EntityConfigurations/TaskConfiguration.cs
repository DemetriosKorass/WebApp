using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = WebApp.DAL.Entities.Task;

namespace WebApp.DAL.EntityConfigurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Task>
    {
        public void Configure(EntityTypeBuilder<Task> builder)
        {
            builder.ToTable("Tasks");

            builder.Property(t => t.Name)
                .HasMaxLength(150)
                .IsRequired();
        }
    }
}
