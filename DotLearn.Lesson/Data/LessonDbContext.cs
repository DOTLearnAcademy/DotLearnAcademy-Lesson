using DotLearn.Lesson.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotLearn.Lesson.Data;

public class LessonDbContext : DbContext
{
    public LessonDbContext(DbContextOptions<LessonDbContext> options) : base(options) { }

    public DbSet<Models.Entities.Lesson> Lessons { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Entities.Lesson>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Title).IsRequired().HasMaxLength(300);
            entity.Property(l => l.Type).IsRequired();
            entity.HasIndex(l => new { l.CourseId, l.OrderIndex });
        });
    }
}
