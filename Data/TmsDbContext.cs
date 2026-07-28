using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

public class TmsDbContext(DbContextOptions<TmsDbContext> options)
    : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    
 public DbSet<Assessment> Assessments => Set<Assessment>();

public DbSet<Certificate> Certificates => Set<Certificate>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Real-world Example: Think of this like an automatic metal detector. 
        // It scans your whole project, finds your individual configuration files (like StudentConfiguration.cs), 
        // and applies their rules instantly so you don't have to type them all here.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);
    }
}