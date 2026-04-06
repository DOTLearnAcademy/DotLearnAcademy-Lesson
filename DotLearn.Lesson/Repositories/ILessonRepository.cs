using DotLearn.Lesson.Models.Entities;

namespace DotLearn.Lesson.Repositories;

public interface ILessonRepository
{
    Task<Models.Entities.Lesson?> GetByIdAsync(Guid id);
    Task<List<Models.Entities.Lesson>> GetByCourseIdAsync(Guid courseId);
    Task AddAsync(Models.Entities.Lesson lesson);
    Task UpdateAsync(Models.Entities.Lesson lesson);
    Task DeleteAsync(Models.Entities.Lesson lesson);
    Task<bool> HasWatchHistoryAsync(Guid lessonId);
    Task BulkUpdateOrderAsync(List<(Guid LessonId, int OrderIndex)> updates);
}
