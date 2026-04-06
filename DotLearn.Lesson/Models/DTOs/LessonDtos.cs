namespace DotLearn.Lesson.Models.DTOs;

public record CreateLessonRequestDto(
    string Title,
    string Type,
    int OrderIndex,
    bool IsPreview = false,
    string? TextContent = null
);

public record UpdateLessonRequestDto(
    string? Title,
    string? Type,
    int? OrderIndex,
    bool? IsPreview,
    string? TextContent
);

public record LessonResponseDto(
    Guid Id,
    Guid CourseId,
    string Title,
    string Type,
    int OrderIndex,
    bool IsPreview,
    string? VideoS3Key,
    string? PdfS3Key,
    string? TextContent,
    int? DurationSeconds,
    DateTime CreatedAt
);

public record UploadConfirmRequestDto(string S3Key, int? DurationSeconds);

public record ReorderItemDto(Guid LessonId, int OrderIndex);

public record ReorderRequestDto(Guid CourseId, List<ReorderItemDto> LessonOrders);
