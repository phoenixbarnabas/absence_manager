using Data;
using Entities.Helpers;
using Entities.Models;
using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped(typeof(Repository<>));
builder.Services.AddSingleton<DtoProvider>();
builder.Services.AddScoped<OfficeBookingLogic>();
builder.Services.AddScoped<OfficeManagementLogic>();
builder.Services.AddScoped<IUserLogic, UserLogic>();
builder.Services.AddScoped<CalendarLogic>();
builder.Services.AddScoped<AbsenceRequestLogic>();
builder.Services.AddScoped<IAppUserResolver, AppUserResolver>();
builder.Services.AddScoped<IMsGraphLogic, MsGraphLogic>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICurrentUserGraphSyncService, CurrentUserGraphSyncService>();
builder.Services.AddScoped<IUserActivityLogger, UserActivityLogger>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IAbsenceRequestActionTokenService, AbsenceRequestActionTokenService>();
builder.Services.AddScoped<IAbsenceRequestEmailService, AbsenceRequestEmailService>();



var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? [])
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AbsenceManagerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("Graph"))
    .AddInMemoryTokenCaches();

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddHealthChecks()
    .AddCheck("api", ()=> HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<AbsenceManagerDbContext>(name: "database", tags: new[] { "ready" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/api/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = WriteHealthCheckResponse
}).AllowAnonymous();

app.MapHealthChecks("/api/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse
}).AllowAnonymous();

app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
}).AllowAnonymous();

static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        durationMs = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description,
            durationMs = entry.Value.Duration.TotalMilliseconds,
            error = entry.Value.Exception?.Message
        })
    };

    return context.Response.WriteAsync(JsonSerializer.Serialize(response));
}

app.Run();