using Bookify.Application.Interfaces;
using Bookify.Infrastructure.Authentication;
using Bookify.Infrastructure.Persistence;
using Bookify.Infrastructure.Persistence.Repositories;
using Bookify.Infrastructure.Services;
using Bookify.Infrastructure.Services.BackgroundJobs;
using Bookify.Infrastructure.Services.Payments;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database — use InMemory for testing when configured, otherwise SQL Server
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
        if (useInMemory)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(configuration.GetConnectionString("DefaultConnection") 
                    ?? "BookifyDb"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b =>
                    {
                        b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        b.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    }));
        }

        // Current User Service (for audit fields)
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IBusinessHoursRepository, BusinessHoursRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRecurringBookingRepository, RecurringBookingRepository>();
        services.AddScoped<IWaitlistRepository, WaitlistRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Authentication
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        // Services
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReviewStatisticsService, ReviewStatisticsService>();
        services.AddScoped<IAISearchService, AISearchService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<ISessionManager, SessionManager>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ISlotGenerator, SlotGenerator>();
        services.AddScoped<IBusinessRuleValidator, BusinessRuleValidator>();
        services.AddScoped<IRecurringBookingGeneratorService, Services.BackgroundJobs.RecurringBookingGeneratorService>();
        services.AddScoped<IWaitlistPromotionService, Services.BackgroundJobs.WaitlistPromotionService>();
        services.AddScoped<SeedService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IVirusScanService, VirusScanService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddSingleton<IOpenTelemetryService, Services.OpenTelemetryService>();

        // ── Caching ──
        services.AddMemoryCache();

        // Use Redis if configured, otherwise fall back to in-memory cache
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "Bookify:";
            });
            services.AddSingleton<ICacheService, DistributedCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        // ── Hangfire Background Jobs ──
        var hangfireConnection = configuration.GetConnectionString("HangfireConnection");
        if (!string.IsNullOrEmpty(hangfireConnection))
        {
            services.AddHangfire(config =>
                config.UseSqlServerStorage(hangfireConnection, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 2;
                options.Queues = new[] { "default", "emails", "sms", "cleanup", "payments" };
            });
        }

        // Background Jobs (Interfaces)
        services.AddScoped<IBackgroundJobScheduler, Services.BackgroundJobs.BackgroundJobScheduler>();
        services.AddScoped<IReminderJob, Services.BackgroundJobs.ReminderJob>();
        services.AddScoped<ICleanupJob, Services.BackgroundJobs.CleanupJob>();
        services.AddScoped<IEmailQueueJob, Services.BackgroundJobs.EmailQueueJob>();
        services.AddScoped<ISmsQueueJob, Services.BackgroundJobs.SmsQueueJob>();
        services.AddScoped<IPaymentRetryJob, Services.BackgroundJobs.PaymentRetryJob>();

        return services;
    }

    /// <summary>
    /// Registers all recurring Hangfire jobs. Call this during application startup.
    /// </summary>
    public static void RegisterRecurringJobs()
    {
        RecurringJob.AddOrUpdate<IReminderJob>(
            "appointment-reminders",
            job => job.ProcessAppointmentRemindersAsync(CancellationToken.None),
            JobCron.EveryFifteenMinutes);

        RecurringJob.AddOrUpdate<ICleanupJob>(
            "cleanup-expired-tokens",
            job => job.CleanExpiredRefreshTokensAsync(CancellationToken.None),
            JobCron.DailyMorning);

        RecurringJob.AddOrUpdate<ICleanupJob>(
            "cleanup-expired-waitlist",
            job => job.ExpireWaitlistEntriesAsync(CancellationToken.None),
            JobCron.Hourly);

        RecurringJob.AddOrUpdate<IEmailQueueJob>(
            "email-queue-processor",
            job => job.ProcessEmailQueueAsync(CancellationToken.None),
            JobCron.EveryFiveMinutes);

        RecurringJob.AddOrUpdate<ISmsQueueJob>(
            "sms-queue-processor",
            job => job.ProcessSmsQueueAsync(CancellationToken.None),
            JobCron.EveryFiveMinutes);

        RecurringJob.AddOrUpdate<IPaymentRetryJob>(
            "payment-retry",
            job => job.RetryFailedPaymentsAsync(CancellationToken.None),
            JobCron.EveryFifteenMinutes);

        RecurringJob.AddOrUpdate<IRecurringBookingGeneratorService>(
            "recurring-booking-generator",
            job => job.GenerateAppointmentsAsync(CancellationToken.None),
            JobCron.DailyMorning);

        RecurringJob.AddOrUpdate<ICleanupJob>(
            "cleanup-soft-deleted",
            job => job.CleanSoftDeletedRecordsAsync(CancellationToken.None),
            JobCron.Weekly);
    }
}
