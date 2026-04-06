using DotLearn.Lesson.Models.DTOs;

namespace DotLearn.Lesson.Services;

public interface ILessonService
{
    Task<LessonResponseDto> CreateAsync(Guid courseId,
        CreateLessonRequestDto request, Guid requesterId);
    Task<List<LessonResponseDto>> GetByCourseIdAsync(
        Guid courseId, bool isEnrolled);
    Task<LessonResponseDto?> GetByIdAsync(
        Guid id, Guid requesterId, string requesterRole);
    Task<LessonResponseDto> UpdateAsync(
        Guid id, UpdateLessonRequestDto request,
        Guid requesterId, string requesterRole);
    Task DeleteAsync(Guid id, Guid requesterId, string requesterRole);
    Task ReorderAsync(ReorderRequestDto request, Guid requesterId);
    Task<string> GetUploadUrlAsync(Guid lessonId, Guid requesterId);
    Task ConfirmUploadAsync(
        Guid lessonId, UploadConfirmRequestDto request, Guid requesterId);
    Task<string> GetVideoUrlAsync(
        Guid lessonId, Guid requesterId, string requesterRole);
}
