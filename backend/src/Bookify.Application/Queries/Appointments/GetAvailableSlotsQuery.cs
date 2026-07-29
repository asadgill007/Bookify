using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Appointments;

public sealed class GetAvailableSlotsQuery : IRequest<Result<List<TimeSlot>>>
{
    public Guid ProviderId { get; init; }
    public Guid? ServiceId { get; init; }
    public Guid BusinessId { get; init; }
    public DateOnly Date { get; init; }
    public int BufferMinutes { get; init; }
}

public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, Result<List<TimeSlot>>>
{
    private readonly ISlotGenerator _slotGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableSlotsQueryHandler(ISlotGenerator slotGenerator, IUnitOfWork unitOfWork)
    {
        _slotGenerator = slotGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<TimeSlot>>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null)
            return Result<List<TimeSlot>>.Failure("Provider not found.", "NOT_FOUND");

        if (!provider.IsActive)
            return Result<List<TimeSlot>>.Failure("Provider is not active.", "PROVIDER_INACTIVE");

        // Resolve BusinessId from provider if not provided
        var businessId = request.BusinessId != Guid.Empty
            ? request.BusinessId
            : provider.BusinessId;

        var slotDuration = 60; // Default duration

        if (request.ServiceId.HasValue)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId.Value, cancellationToken);
            if (service == null)
                return Result<List<TimeSlot>>.Failure("Service not found.", "NOT_FOUND");

            slotDuration = service.DurationMinutes;
        }

        var generationRequest = new SlotGenerationRequest
        {
            BusinessId = businessId,
            ProviderId = request.ProviderId,
            ServiceId = request.ServiceId,
            Date = request.Date,
            SlotDurationMinutes = slotDuration,
            BufferMinutes = request.BufferMinutes
        };

        var slots = await _slotGenerator.GenerateSlotsAsync(generationRequest, cancellationToken);

        return Result<List<TimeSlot>>.Success(slots);
    }
}
