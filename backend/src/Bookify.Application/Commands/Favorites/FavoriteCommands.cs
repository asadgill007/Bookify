using Bookify.Application.Common;
using Bookify.Application.DTOs.Businesses;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Favorites;

/// <summary>Add a business to the customer's favorites.</summary>
public sealed record AddFavoriteCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public Guid BusinessId { get; init; }
}

public sealed class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    public AddFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();
    }
}

public sealed class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddFavoriteCommandHandler> _logger;

    public AddFavoriteCommandHandler(IUnitOfWork unitOfWork, ILogger<AddFavoriteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        if (await _unitOfWork.Favorites.IsFavoriteAsync(request.UserId, request.BusinessId, cancellationToken))
            return Result.Success(); // idempotent

        await _unitOfWork.Favorites.AddAsync(new FavoriteBusiness(request.UserId, request.BusinessId), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} favorited business {BusinessId}", request.UserId, request.BusinessId);
        return Result.Success();
    }
}

/// <summary>Remove a business from the customer's favorites.</summary>
public sealed record RemoveFavoriteCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public Guid BusinessId { get; init; }
}

public sealed class RemoveFavoriteCommandValidator : AbstractValidator<RemoveFavoriteCommand>
{
    public RemoveFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();
    }
}

public sealed class RemoveFavoriteCommandHandler : IRequestHandler<RemoveFavoriteCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFavoriteCommandHandler> _logger;

    public RemoveFavoriteCommandHandler(IUnitOfWork unitOfWork, ILogger<RemoveFavoriteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var favorite = await _unitOfWork.Favorites.GetAsync(request.UserId, request.BusinessId, cancellationToken);
        if (favorite == null)
            return Result.Success(); // idempotent

        await _unitOfWork.Favorites.DeleteAsync(favorite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} unfavorited business {BusinessId}", request.UserId, request.BusinessId);
        return Result.Success();
    }
}
