using Bookify.Application.Interfaces;
using Bookify.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;

    public IUserRepository Users { get; }
    public IBusinessRepository Businesses { get; }
    public IProviderRepository Providers { get; }
    public IServiceRepository Services { get; }
    public IBusinessHoursRepository BusinessHours { get; }
    public IAppointmentRepository Appointments { get; }
    public IReviewRepository Reviews { get; }
    public IPaymentRepository Payments { get; }
    public INotificationRepository Notifications { get; }
    public ICategoryRepository Categories { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IRecurringBookingRepository RecurringBookings { get; }
    public IWaitlistRepository Waitlist { get; }
    public IDocumentRepository Documents { get; }

    public UnitOfWork(
        AppDbContext context,
        IMediator mediator,
        ILogger<UnitOfWork> logger,
        IUserRepository users,
        IBusinessRepository businesses,
        IProviderRepository providers,
        IServiceRepository services,
        IBusinessHoursRepository businessHours,
        IAppointmentRepository appointments,
        IReviewRepository reviews,
        IPaymentRepository payments,
        INotificationRepository notifications,
        ICategoryRepository categories,
        IRefreshTokenRepository refreshTokens,
        IRecurringBookingRepository recurringBookings,
        IWaitlistRepository waitlist,
        IDocumentRepository documents)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
        Users = users;
        Businesses = businesses;
        Providers = providers;
        Services = services;
        BusinessHours = businessHours;
        Appointments = appointments;
        Reviews = reviews;
        Payments = payments;
        Notifications = notifications;
        Categories = categories;
        RefreshTokens = refreshTokens;
        RecurringBookings = recurringBookings;
        Waitlist = waitlist;
        Documents = documents;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectAndClearDomainEvents();
        var result = await _context.SaveChangesAsync(cancellationToken);

        if (domainEvents.Count > 0)
        {
            await PublishDomainEventsAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    private List<IDomainEvent> CollectAndClearDomainEvents()
    {
        var domainEvents = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        if (domainEvents.Count > 0)
        {
            foreach (var entry in _context.ChangeTracker.Entries<BaseEntity>())
            {
                entry.Entity.ClearDomainEvents();
            }
        }

        return domainEvents;
    }

    private async Task PublishDomainEventsAsync(List<IDomainEvent> events, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in events)
        {
            try
            {
                await _mediator.Publish(domainEvent, cancellationToken);
                _logger.LogInformation(
                    "Published domain event {EventType} at {Time}",
                    domainEvent.GetType().Name,
                    DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish domain event {EventType}: {ErrorMessage}",
                    domainEvent.GetType().Name,
                    ex.Message);
                // Don't rethrow — domain event failures should not roll back the transaction
            }
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var domainEvents = CollectAndClearDomainEvents();
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;

            if (domainEvents.Count > 0)
            {
                await PublishDomainEventsAsync(domainEvents, cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
