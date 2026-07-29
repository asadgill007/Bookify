using Bookify.Application.Interfaces;
using Bookify.Infrastructure.Authentication;
using Bookify.Infrastructure.Persistence;
using Bookify.Infrastructure.Persistence.Repositories;
using Bookify.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
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
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
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
        services.AddScoped<IPaymentService, PaymentService>();
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
        services.AddScoped<IVirusScanService, NoVirusScanService>();
        services.AddSingleton<IOpenTelemetryService, Services.OpenTelemetryService>();

        // Caching
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Background Jobs (Interfaces + Scheduler)
        services.AddScoped<IBackgroundJobScheduler, Services.BackgroundJobs.BackgroundJobScheduler>();
        services.AddScoped<IReminderJob, Services.BackgroundJobs.ReminderJob>();
        services.AddScoped<ICleanupJob, Services.BackgroundJobs.CleanupJob>();
        services.AddScoped<IEmailQueueJob, Services.BackgroundJobs.EmailQueueJob>();
        services.AddScoped<ISmsQueueJob, Services.BackgroundJobs.SmsQueueJob>();
        services.AddScoped<IPaymentRetryJob, Services.BackgroundJobs.PaymentRetryJob>();

        return services;
    }
}
