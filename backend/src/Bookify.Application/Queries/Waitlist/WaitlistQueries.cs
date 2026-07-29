using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Waitlist;

public sealed record GetBusinessWaitlistQuery : IRequest<Result<PaginatedList<WaitlistEntryDto>>>
{
    public Guid BusinessId { get; init; }
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record GetProviderWaitlistQuery : IRequest<Result<PaginatedList<WaitlistEntryDto>>>
{
    public Guid ProviderId { get; init; }
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record GetCustomerWaitlistQuery : IRequest<Result<PaginatedList<WaitlistEntryDto>>>
{
    public Guid CustomerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record GetWaitlistStatisticsQuery : IRequest<Result<WaitlistStatistics>>
{
    public Guid BusinessId { get; init; }
    public Guid UserId { get; init; }
}

public class WaitlistEntryDto
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid CustomerId { get; set; }
    public string AppointmentDate { get; set; } = string.Empty;
    public string? PreferredStartTime { get; set; }
    public string? PreferredEndTime { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int Position { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class GetBusinessWaitlistQueryHandler : IRequestHandler<GetBusinessWaitlistQuery, Result<PaginatedList<WaitlistEntryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetBusinessWaitlistQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<WaitlistEntryDto>>> Handle(GetBusinessWaitlistQuery request, CancellationToken cancellationToken)
    {
        var entries = await _unitOfWork.Waitlist.GetBusinessWaitlistAsync(request.BusinessId, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Waitlist.GetBusinessWaitlistCountAsync(request.BusinessId, cancellationToken);

        var items = entries.Select(MapToDto).ToList();
        return Result<PaginatedList<WaitlistEntryDto>>.Success(new PaginatedList<WaitlistEntryDto>(items, request.Page, request.PageSize, total));
    }

    private static WaitlistEntryDto MapToDto(Domain.Entities.WaitlistEntry e)
    {
        return new WaitlistEntryDto
        {
            Id = e.Id, BusinessId = e.BusinessId, ProviderId = e.ProviderId,
            ServiceId = e.ServiceId, CustomerId = e.CustomerId,
            AppointmentDate = e.AppointmentDate.ToString("yyyy-MM-dd"),
            PreferredStartTime = e.PreferredStartTime?.ToString(),
            PreferredEndTime = e.PreferredEndTime?.ToString(),
            Notes = e.Notes, Status = e.Status.ToString(), Priority = e.Priority,
            CustomerName = e.Customer != null ? $"{e.Customer.FirstName} {e.Customer.LastName}" : "",
            ProviderName = e.Provider?.User != null ? $"{e.Provider.User.FirstName} {e.Provider.User.LastName}" : "",
            ServiceName = e.Service?.Name ?? "", CreatedAt = e.CreatedAt
        };
    }
}

public sealed class GetProviderWaitlistQueryHandler : IRequestHandler<GetProviderWaitlistQuery, Result<PaginatedList<WaitlistEntryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetProviderWaitlistQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<WaitlistEntryDto>>> Handle(GetProviderWaitlistQuery request, CancellationToken cancellationToken)
    {
        var entries = await _unitOfWork.Waitlist.GetProviderWaitlistAsync(request.ProviderId, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Waitlist.GetProviderWaitlistCountAsync(request.ProviderId, cancellationToken);

        var items = entries.Select(e => new WaitlistEntryDto
        {
            Id = e.Id, Status = e.Status.ToString(),
            AppointmentDate = e.AppointmentDate.ToString("yyyy-MM-dd"),
            CustomerName = e.Customer != null ? $"{e.Customer.FirstName} {e.Customer.LastName}" : "",
            ServiceName = e.Service?.Name ?? "", Priority = e.Priority,
            Notes = e.Notes, CreatedAt = e.CreatedAt
        }).ToList();

        return Result<PaginatedList<WaitlistEntryDto>>.Success(new PaginatedList<WaitlistEntryDto>(items, request.Page, request.PageSize, total));
    }
}

public sealed class GetCustomerWaitlistQueryHandler : IRequestHandler<GetCustomerWaitlistQuery, Result<PaginatedList<WaitlistEntryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetCustomerWaitlistQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<WaitlistEntryDto>>> Handle(GetCustomerWaitlistQuery request, CancellationToken cancellationToken)
    {
        var entries = await _unitOfWork.Waitlist.GetCustomerWaitlistAsync(request.CustomerId, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Waitlist.GetCustomerWaitlistCountAsync(request.CustomerId, cancellationToken);

        var items = entries.Select(e => new WaitlistEntryDto
        {
            Id = e.Id, Status = e.Status.ToString(),
            AppointmentDate = e.AppointmentDate.ToString("yyyy-MM-dd"),
            ProviderName = e.Provider?.User != null ? $"{e.Provider.User.FirstName} {e.Provider.User.LastName}" : "",
            ServiceName = e.Service?.Name ?? "", BusinessId = e.BusinessId,
            Notes = e.Notes, Priority = e.Priority, CreatedAt = e.CreatedAt
        }).ToList();

        return Result<PaginatedList<WaitlistEntryDto>>.Success(new PaginatedList<WaitlistEntryDto>(items, request.Page, request.PageSize, total));
    }
}

public sealed class GetWaitlistStatisticsQueryHandler : IRequestHandler<GetWaitlistStatisticsQuery, Result<WaitlistStatistics>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetWaitlistStatisticsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<WaitlistStatistics>> Handle(GetWaitlistStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _unitOfWork.Waitlist.GetStatisticsAsync(request.BusinessId, cancellationToken);
        return Result<WaitlistStatistics>.Success(stats);
    }
}
