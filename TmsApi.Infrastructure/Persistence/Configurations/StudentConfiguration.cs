using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            // 1. Primary Key (Identity Card)
            builder.HasKey(s => s.Id);

            // 2. Rules for properties
            builder.Property(s => s.Name)
                   .IsRequired()            // Real-world: You cannot join a school without a name (Cannot be blank)
                   .HasMaxLength(100); 
                        // Real-world: Names cannot be longer than 100 letters
                        // 1. Configure Shadow Property for LastUpdated
            builder.Property<DateTime>("LastUpdated")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // 2. Configure Concurrency Token mapped automatically to xmin system column
        builder.Property(s => s.Version)
               .IsRowVersion();
        builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
