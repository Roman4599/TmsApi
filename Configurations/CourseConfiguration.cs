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

            // 2. Rules for properties
            builder.Property(c => c.Title)
                   .IsRequired()            // Real-world: A course must have a title (e.g., "Math 101")
                   .HasMaxLength(200);      // Real-world: Limit title length so it doesn't break UI layouts
        }
    }
}