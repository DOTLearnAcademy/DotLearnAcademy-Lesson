using DotLearn.Lesson.Data;
using DotLearn.Lesson.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Amazon;
using Amazon.S3;
using Kralizek.Extensions.Configuration;
using DotLearn.Lesson.Repositories;
using DotLearn.Lesson.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// AWS Secrets Manager
builder.Configuration.AddSecretsManager(region: RegionEndpoint.APSoutheast2);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LessonDbContext>(options =>
    options.UseSqlServer(connStr));

builder.Services.AddHealthChecks().AddSqlServer(connStr, name: "sqlserver");

builder.Services.AddDefaultAWSOptions(new Amazon.Extensions.NETCore.Setup.AWSOptions
{
    Region = RegionEndpoint.APSoutheast2
});
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonService, LessonService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication & Authorization (Placeholder)
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandler>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
