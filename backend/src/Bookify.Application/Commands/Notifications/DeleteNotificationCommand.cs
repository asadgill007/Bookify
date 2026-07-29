using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Notifications;

public sealed record DeleteNotificationCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public Guid NotificationId { get; init; }
}

public sealed class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public sealed class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteNotificationCommandHandler> _logger;

    public DeleteNotificationCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteNotificationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification == null)
            return Result.Failure("Notification not found.", "NOT_FOUND");

        if (notification.UserId != request.UserId)
            return Result.Failure("Access denied.", "FORBIDDEN");

        await _unitOfWork.Notifications.DeleteAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} deleted notification {NotificationId}", request.UserId, request.NotificationId);
        return Result.Success();
    }
}
