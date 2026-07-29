using FluentValidation;

namespace Bookify.Application.DTOs.Appointments;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string ServiceName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentRequest
{
    public Guid ProviderId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid BusinessId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? CustomerNotes { get; set; }
}

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty().GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime);
    }
}

public class CancelAppointmentRequest
{
    public string? Reason { get; set; }
}

public class RescheduleAppointmentRequest
{
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
}

public class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
{
    public RescheduleAppointmentRequestValidator()
    {
        RuleFor(x => x.NewStartTime).NotEmpty().GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.NewEndTime).NotEmpty().GreaterThan(x => x.NewStartTime);
    }
}
