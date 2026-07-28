using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            // Set the main primary key for the enrollment row
            builder.HasKey(e => e.Id);

            // Task 1: Connect Student to Enrollment (One-to-Many)
            builder.HasOne(e => e.Student)
                   .WithMany(s => s.Enrollments)
                   .HasForeignKey(e => e.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Task 1: Connect Course to Enrollment (One-to-Many)
            builder.HasOne(e => e.Course)
                   .WithMany(c => c.Enrollments)
                   .HasForeignKey(e => e.CourseId)
                   // Task 2 Comment: Restrict prevents accidental data loss of a student's academic history if a course is deleted.
                   .OnDelete(DeleteBehavior.Restrict);

            // Configure properties
            builder.Property(e => e.Grade)
                   .IsRequired()
                   .HasMaxLength(2);
        }
    }
}