using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.DAL.Entities;

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
            builder.HasIndex(u => u.Email)
                .IsUnique();
            builder.HasMany(u => u.Tasks)
                .WithMany(t => t.Users);
        }
    }
}