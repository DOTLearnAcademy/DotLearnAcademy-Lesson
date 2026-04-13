using Amazon.S3;
using Amazon.S3.Model;
using DotLearn.Lesson.Models.DTOs;
using DotLearn.Lesson.Models.Entities;
using DotLearn.Lesson.Repositories;

namespace DotLearn.Lesson.Services;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _repo;
    private readonly IAmazonS3 _s3;
    private readonly IConfiguration _config;

    public LessonService(ILessonRepository repo, IAmazonS3 s3, IConfiguration config)
    {
        _repo = repo;
        _s3 = s3;
        _config = config;
    }

    public async Task<LessonResponseDto> CreateAsync(
        Guid courseId, CreateLessonRequestDto request, Guid requesterId)
    {
        var lesson = new Models.Entities.Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = request.Title,
            Type = Enum.Parse<ContentType>(request.Type, ignoreCase: true),
            OrderIndex = request.OrderIndex,
            IsPreview = request.IsPreview,
            TextContent = request.TextContent
        };

        await _repo.AddAsync(lesson);
        return MapToDto(lesson);
    }

    public async Task<List<LessonResponseDto>> GetByCourseIdAsync(
        Guid courseId, bool isEnrolled)
    {
        var lessons = await _repo.GetByCourseIdAsync(courseId);

        // Non-enrolled users only see preview lessons
        if (!isEnrolled)
            lessons = lessons.Where(l => l.IsPreview).ToList();

        return lessons.Select(MapToDto).ToList();
    }

    public async Task<LessonResponseDto?> GetByIdAsync(
        Guid id, Guid requesterId, string requesterRole)
    {
        var lesson = await _repo.GetByIdAsync(id);
        if (lesson == null) return null;

        // Access control handled in controller via enrollment check
        return MapToDto(lesson);
    }

    public async Task<LessonResponseDto> UpdateAsync(
        Guid id, UpdateLessonRequestDto request,
        Guid requesterId, string requesterRole)
    {
        var lesson = await GetOrThrow(id);

        if (request.Title != null) lesson.Title = request.Title;
        if (request.OrderIndex.HasValue) lesson.OrderIndex = request.OrderIndex.Value;
        if (request.IsPreview.HasValue) lesson.IsPreview = request.IsPreview.Value;
        if (request.TextContent != null) lesson.TextContent = request.TextContent;
        if (request.Type != null)
            lesson.Type = Enum.Parse<ContentType>(request.Type, ignoreCase: true);

        await _repo.UpdateAsync(lesson);
        return MapToDto(lesson);
    }

    public async Task DeleteAsync(
        Guid id, Guid requesterId, string requesterRole)
    {
        var lesson = await GetOrThrow(id);

        if (await _repo.HasWatchHistoryAsync(id))
            throw new InvalidOperationException(
                "Cannot delete a lesson that students have watched.");

        await _repo.DeleteAsync(lesson);
    }

    public async Task ReorderAsync(ReorderRequestDto request, Guid requesterId)
    {
        var updates = request.LessonOrders
            .Select(x => (x.LessonId, x.OrderIndex))
            .ToList();

        await _repo.BulkUpdateOrderAsync(updates);
    }

    public async Task<string> GetUploadUrlAsync(Guid lessonId, Guid requesterId)
    {
        var lesson = await GetOrThrow(lessonId);
        var bucket = _config["AWS:VideosBucket"] ?? "dotlearn-videos-dev";
        var key = $"lessons/{lessonId}/video.mp4";

        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        return _s3.GetPreSignedURL(presignRequest);
    }

    public async Task ConfirmUploadAsync(
        Guid lessonId, UploadConfirmRequestDto request, Guid requesterId)
    {
        var lesson = await GetOrThrow(lessonId);
        lesson.VideoS3Key = request.S3Key;
        lesson.DurationSeconds = request.DurationSeconds;
        await _repo.UpdateAsync(lesson);
    }

    public async Task<string> GetVideoUrlAsync(
        Guid lessonId, Guid requesterId, string requesterRole)
    {
        var lesson = await GetOrThrow(lessonId);

        // TODO: Call Enrollment Service check in Phase 3
        // For now allow access if lesson exists

        var bucket = _config["AWS:VideosBucket"] ?? "dotlearn-videos-dev";

        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = lesson.VideoS3Key!,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        return _s3.GetPreSignedURL(presignRequest);
    }

    // ── Helpers ──────────────────────────────────────────────────
    private async Task<Models.Entities.Lesson> GetOrThrow(Guid id)
    {
        var lesson = await _repo.GetByIdAsync(id);
        if (lesson == null)
            throw new KeyNotFoundException($"Lesson {id} not found.");
        return lesson;
    }

    private static LessonResponseDto MapToDto(Models.Entities.Lesson l) => new(
        l.Id, l.CourseId, l.Title, l.Type.ToString(),
        l.OrderIndex, l.IsPreview, l.VideoS3Key,
        l.PdfS3Key, l.TextContent, l.DurationSeconds, l.CreatedAt
    );
}
