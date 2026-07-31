using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : BaseRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Appointment?> GetByBookingReferenceAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(a => a.Provider).ThenInclude(p => p.User)
            .Include(a => a.Service)
            .Include(a => a.Business)
            .FirstOrDefaultAsync(a => a.BookingReference == bookingReference, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetUserAppointmentsAsync(
        Guid userId,
        bool isCustomer,
        AppointmentStatus? statusFilter,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(a => a.Provider).ThenInclude(p => p.User)
            .Include(a => a.Service)
            .Include(a => a.Business)
            .AsQueryable();

        if (isCustomer)
            query = query.Where(a => a.CustomerId == userId);
        else
            query = query.Where(a => a.Provider.UserId == userId);

        if (statusFilter.HasValue)
            query = query.Where(a => a.Status == statusFilter.Value);

        if (from.HasValue)
            query = query.Where(a => a.StartTime >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.StartTime <= to.Value);

        query = query.OrderByDescending(a => a.StartTime);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasConflictAsync(
        Guid providerId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(a =>
            a.ProviderId == providerId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.NoShow &&
            a.StartTime < endTime &&
            a.EndTime > startTime);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DateTime>> GetBookedSlotsAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await DbSet
            .AsNoTracking()
            .Where(a =>
                a.ProviderId == providerId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                a.StartTime >= dayStart &&
                a.StartTime <= dayEnd)
            .Select(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByBusinessIdAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.BusinessId == businessId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByBusinessIdDateRangeAsync(
        Guid businessId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.BusinessId == businessId
                     && a.StartTime >= from
                     && a.StartTime < to)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetWithCustomerAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByStatusDateRangeAsync(
        AppointmentStatus status,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.Status == status
                     && a.StartTime >= from
                     && a.StartTime <= to)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(AppointmentStatus? statusFilter = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (statusFilter.HasValue)
            query = query.Where(a => a.Status == statusFilter.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<decimal> GetCompletedRevenueAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Status == AppointmentStatus.Completed)
            .SumAsync(a => a.TotalAmount, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetFutureByRecurringBookingAsync(
        Guid recurringBookingId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(a => a.RecurringBookingId == recurringBookingId
                     && a.StartTime > now
                     && a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }
}