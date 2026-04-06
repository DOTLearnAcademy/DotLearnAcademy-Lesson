namespace DotLearn.Lesson.Models.Entities;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = null!;
    public ContentType Type { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPreview { get; set; } = false;
    public string? VideoS3Key { get; set; }
    public string? PdfS3Key { get; set; }
    public string? TextContent { get; set; }
    public int? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ContentType
{
    Video = 0,
    PDF = 1,
    Text = 2
}
