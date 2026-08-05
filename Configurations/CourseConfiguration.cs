using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            // 1. Primary Key 
            builder.HasKey(c => c.Id);

            // 2. Rules for Code (NEW - ADD THIS)
            builder.Property(c => c.Code)
                   .IsRequired()            // Code must always be provided
                   .HasMaxLength(10);       // Keep codes short (e.g., "CS101")

            // 3. Rules for Title
            builder.Property(c => c.Title)
                   .IsRequired()            
                   .HasMaxLength(200);      

            // 4. Rule for MaxCapacity (NEW - ADD THIS)
            builder.Property(c => c.MaxCapacity);

            // 5. UNIQUE INDEX on Code (MOST IMPORTANT - ADD THIS)
            // This prevents two courses from having the same Code in the database
            builder.HasIndex(c => c.Code).IsUnique();

            // 6. Soft-delete filter (Keep this as-is)
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}