using DotLearn.Lesson.Models.DTOs;
using DotLearn.Lesson.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DotLearn.Lesson.Controllers;

[ApiController]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    // GET /api/courses/{courseId}/lessons
    [HttpGet("api/courses/{courseId}/lessons")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        // Non-authenticated users see preview only
        var isEnrolled = User.Identity?.IsAuthenticated ?? false;
        var lessons = await _lessonService.GetByCourseIdAsync(courseId, isEnrolled);
        return Ok(lessons);
    }

    // GET /api/lessons/{id}
    [HttpGet("api/lessons/{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var lesson = await _lessonService.GetByIdAsync(
            id, GetUserId(), GetUserRole());
        if (lesson == null) return NotFound(new { error = "Lesson not found." });
        return Ok(lesson);
    }

    // POST /api/courses/{courseId}/lessons
    [HttpPost("api/courses/{courseId}/lessons")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Create(
        Guid courseId, [FromBody] CreateLessonRequestDto request)
    {
        try
        {
            var result = await _lessonService.CreateAsync(
                courseId, request, GetUserId());
            return StatusCode(201, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT /api/lessons/{id}
    [HttpPut("api/lessons/{id}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateLessonRequestDto request)
    {
        try
        {
            var result = await _lessonService.UpdateAsync(
                id, request, GetUserId(), GetUserRole());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // DELETE /api/lessons/{id}
    [HttpDelete("api/lessons/{id}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _lessonService.DeleteAsync(id, GetUserId(), GetUserRole());
            return Ok(new { message = "Lesson deleted." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // PUT /api/lessons/reorder
    [HttpPut("api/lessons/reorder")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Reorder([FromBody] ReorderRequestDto request)
    {
        try
        {
            await _lessonService.ReorderAsync(request, GetUserId());
            return Ok(new { message = "Lessons reordered." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // POST /api/lessons/{id}/upload-url
    [HttpPost("api/lessons/{id}/upload-url")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> GetUploadUrl(Guid id)
    {
        try
        {
            var url = await _lessonService.GetUploadUrlAsync(id, GetUserId());
            return Ok(new { uploadUrl = url });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // POST /api/lessons/{id}/upload-confirm
    [HttpPost("api/lessons/{id}/upload-confirm")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> ConfirmUpload(
        Guid id, [FromBody] UploadConfirmRequestDto request)
    {
        try
        {
            await _lessonService.ConfirmUploadAsync(id, request, GetUserId());
            return Ok(new { message = "Upload confirmed." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // GET /api/lessons/{id}/video-url
    [HttpGet("api/lessons/{id}/video-url")]
    [Authorize]
    public async Task<IActionResult> GetVideoUrl(Guid id)
    {
        try
        {
            var url = await _lessonService.GetVideoUrlAsync(
                id, GetUserId(), GetUserRole());
            return Ok(new { videoUrl = url });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ── Helpers ──────────────────────────────────────────────────
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));

    private string GetUserRole() =>
        User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("Role not found in token.");
}
