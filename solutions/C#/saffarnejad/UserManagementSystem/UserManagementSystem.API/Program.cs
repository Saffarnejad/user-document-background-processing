using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using UserManagementSystem.BackgroundJobs.Jobs;
using UserManagementSystem.Core.Interfaces.Repositories;
using UserManagementSystem.Core.Interfaces.Services;
using UserManagementSystem.Infrastructure.Data;
using UserManagementSystem.Infrastructure.Repositories;
using UserManagementSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseMemoryStorage()); // Use SQL Server in production

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

builder.Services.AddScoped<WelcomeNotificationJob>();
builder.Services.AddScoped<DocumentProcessingJob>();
builder.Services.AddScoped<CompletionNotificationJob>();
builder.Services.AddScoped<NightlyCleanupJob>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

RecurringJob.AddOrUpdate<NightlyCleanupJob>(
    "nightly-cleanup",
    j => j.Execute("nightly-cleanup-" + DateTime.UtcNow.ToString("yyyyMMdd")),
    Cron.Daily); // Runs daily at midnight

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In production, implement proper authentication/authorization
        return true; // Allow all in development
    }
}
