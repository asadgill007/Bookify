using MediatR;

namespace Bookify.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
