using DotLearn.Lesson.Data;
using DotLearn.Lesson.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotLearn.Lesson.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly LessonDbContext _context;

    public LessonRepository(LessonDbContext context)
    {
        _context = context;
    }

    public async Task<Models.Entities.Lesson?> GetByIdAsync(Guid id) =>
        await _context.Lessons.FindAsync(id);

    public async Task<List<Models.Entities.Lesson>> GetByCourseIdAsync(Guid courseId) =>
        await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();

    public async Task AddAsync(Models.Entities.Lesson lesson)
    {
        await _context.Lessons.AddAsync(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Models.Entities.Lesson lesson)
    {
        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Models.Entities.Lesson lesson)
    {
        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasWatchHistoryAsync(Guid lessonId)
    {
        // TODO: Check Progress Service in Phase 4
        // For now return false to allow deletion during development
        return false;
    }

    public async Task BulkUpdateOrderAsync(
        List<(Guid LessonId, int OrderIndex)> updates)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        foreach (var (lessonId, orderIndex) in updates)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson != null)
                lesson.OrderIndex = orderIndex;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
