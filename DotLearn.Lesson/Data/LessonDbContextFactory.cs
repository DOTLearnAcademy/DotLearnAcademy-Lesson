using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotLearn.Lesson.Data;

public class LessonDbContextFactory : IDesignTimeDbContextFactory<LessonDbContext>
{
    public LessonDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LessonDbContext>();
        var connStr = "Server=dotlearn-db.c7ge68ueyfep.ap-southeast-2.rds.amazonaws.com,1433;Database=ContentDb;User Id=admin;Password=DOTLearn@123;TrustServerCertificate=True";
        optionsBuilder.UseSqlServer(connStr);
        return new LessonDbContext(optionsBuilder.Options);
    }
}
