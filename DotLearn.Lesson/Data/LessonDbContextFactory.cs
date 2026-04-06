using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotLearn.Lesson.Data;

public class LessonDbContextFactory : IDesignTimeDbContextFactory<LessonDbContext>
{
    public LessonDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LessonDbContext>();
        optionsBuilder.UseSqlServer("Server=placeholder;Database=ContentDb;");
        return new LessonDbContext(optionsBuilder.Options);
    }
}
