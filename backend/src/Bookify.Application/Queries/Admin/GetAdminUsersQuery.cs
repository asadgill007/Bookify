using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using MediatR;

namespace Bookify.Application.Queries.Admin;

public sealed record GetAdminUsersQuery : IRequest<Result<PaginatedList<AdminUserDto>>>
{
    public string? Role { get; init; }
    public bool? Suspended { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, Result<PaginatedList<AdminUserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminUsersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<AdminUserDto>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        UserRole? roleFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<UserRole>(request.Role, true, out var parsed))
            roleFilter = parsed;

        var users = await _unitOfWork.Users.GetFilteredAsync(
            roleFilter, request.Suspended, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Users.GetFilteredCountAsync(
            roleFilter, request.Suspended, cancellationToken);

        var items = users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role.ToString(),
            IsSuspended = u.IsSuspended,
            SuspensionReason = u.SuspensionReason,
            SuspendedAt = u.SuspendedAt,
            AvatarUrl = u.AvatarUrl,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt,
            IsDeleted = u.IsDeleted
        }).ToList();

        return Result<PaginatedList<AdminUserDto>>.Success(
            new PaginatedList<AdminUserDto>(items, request.Page, request.PageSize, total));
    }
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsSuspended { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
