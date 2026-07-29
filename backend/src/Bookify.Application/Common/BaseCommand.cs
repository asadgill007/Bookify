using MediatR;

namespace Bookify.Application.Common;

public abstract class BaseCommand<TResponse> : IRequest<Result<TResponse>>
{
}

public abstract class BaseCommand : IRequest<Result>
{
}

public abstract class BaseQuery<TResponse> : IRequest<Result<TResponse>>
{
}

public abstract class BasePagedQuery<TResponse> : PagedQuery, IRequest<Result<PaginatedList<TResponse>>>
{
}
