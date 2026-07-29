using Bookify.Domain.Common;

namespace Bookify.Domain.DomainEvents;

public sealed record AppointmentCreatedEvent(
    Guid AppointmentId,
    Guid CustomerId,
    Guid BusinessId,
    Guid ProviderId,
    Guid ServiceId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount,
    DateTime OccurredOn) : IDomainEvent;

public sealed record AppointmentConfirmedEvent(
    Guid AppointmentId,
    Guid CustomerId,
    Guid BusinessId,
    DateTime OccurredOn) : IDomainEvent;

public sealed record AppointmentCancelledEvent(
    Guid AppointmentId,
    Guid CustomerId,
    Guid BusinessId,
    string? Reason,
    DateTime OccurredOn) : IDomainEvent;

public sealed record AppointmentCompletedEvent(
    Guid AppointmentId,
    Guid CustomerId,
    Guid ProviderId,
    Guid BusinessId,
    DateTime OccurredOn) : IDomainEvent;

public sealed record ReviewSubmittedEvent(
    Guid ReviewId,
    Guid AppointmentId,
    Guid BusinessId,
    int Rating,
    DateTime OccurredOn) : IDomainEvent;

public sealed record PaymentCapturedEvent(
    Guid PaymentId,
    Guid AppointmentId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime OccurredOn) : IDomainEvent;

public sealed record BusinessVerifiedEvent(
    Guid BusinessId,
    Guid OwnerId,
    DateTime OccurredOn) : IDomainEvent;
