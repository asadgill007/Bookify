using Bookify.Application.Commands.Appointments;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Bookify.Application.Tests;

public class CreateAppointmentCommandValidatorTests
{
    private readonly CreateAppointmentCommandValidator _sut = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new CreateAppointmentCommand
        {
            CustomerId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            CustomerNotes = "First visit"
        };

        var result = _sut.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyCustomerId_FailsValidation()
    {
        var command = new CreateAppointmentCommand
        {
            CustomerId = Guid.Empty,
            ProviderId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void EmptyProviderId_FailsValidation()
    {
        var command = new CreateAppointmentCommand
        {
            CustomerId = Guid.NewGuid(),
            ProviderId = Guid.Empty,
            ServiceId = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
    }

    [Fact]
    public void StartTimeInPast_FailsValidation()
    {
        var command = new CreateAppointmentCommand
        {
            CustomerId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1)
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void EndTimeBeforeStartTime_FailsValidation()
    {
        var command = new CreateAppointmentCommand
        {
            CustomerId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(-1)
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void CustomerNotesTooLong_FailsValidation()
    {
        var command = new CreateAppointmentCommand
        {
            CustomerId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            CustomerNotes = new string('x', 1001)
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerNotes);
    }

    [Fact]
    public void StartAndEndTimeSame_FailsValidation()
    {
        var sameTime = DateTime.UtcNow.AddDays(1);
        var command = new CreateAppointmentCommand
        {
            CustomerId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            StartTime = sameTime,
            EndTime = sameTime // Equal, not greater
        };

        var result = _sut.TestValidate(command);

        // GreaterThan demands strictly greater
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("End time must be after start time.");
    }
}
