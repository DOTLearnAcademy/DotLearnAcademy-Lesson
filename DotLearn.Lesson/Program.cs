using DotLearn.Lesson.Data;
using DotLearn.Lesson.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using Amazon.S3;
using DotLearn.Lesson.Repositories;
using DotLearn.Lesson.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// AWS Secrets Manager (Only in non-Development environments)
if (!builder.Environment.IsDevelopment())
{
    // // builder.Configuration.AddSecretsManager(region: Amazon.RegionEndpoint.APSoutheast2);
}

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LessonDbContext>(options =>
    options.UseSqlServer(connStr, sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddHealthChecks().AddSqlServer(connStr, name: "sqlserver");

builder.Services.AddDefaultAWSOptions(new Amazon.Extensions.NETCore.Setup.AWSOptions
{
    Region = Amazon.RegionEndpoint.APSoutheast2
});
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonService, LessonService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication & Authorization — manual JWKS loading (no OIDC discovery needed)
var jwksUri = builder.Configuration["Auth:JwksUri"]
    ?? "http://auth/auth/.well-known/jwks.json";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "dotlearn-auth",
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                using var http = new HttpClient();
                var json = http.GetStringAsync(jwksUri).GetAwaiter().GetResult();
                var jwks = new JsonWebKeySet(json);
                return jwks.GetSigningKeys();
            },
            NameClaimType = "sub",
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// CORS — allow localhost dev + EC2 production
builder.Services.AddCors(options =>
{
    options.AddPolicy("DotLearnPolicy", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://3.27.174.183.nip.io",
                "https://3.27.174.183.nip.io",
                builder.Configuration["AllowedOrigins:Ec2"] ?? "",
                builder.Configuration["AllowedOrigins:CloudFront"] ?? "")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

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

app.UseCors("DotLearnPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();


