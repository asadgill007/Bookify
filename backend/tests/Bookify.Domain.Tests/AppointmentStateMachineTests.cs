using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using FluentAssertions;

namespace Bookify.Domain.Tests;

public class AppointmentStateMachineTests
{
    private static Appointment CreatePendingAppointment()
    {
        return new Appointment(
            "BOK-TEST01",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            100.00m);
    }

    [Fact]
    public void Constructor_WithValidData_SetsPendingStatus()
    {
        var appointment = CreatePendingAppointment();

        appointment.Status.Should().Be(AppointmentStatus.Pending);
        appointment.BookingReference.Should().Be("BOK-TEST01");
        appointment.TotalAmount.Should().Be(100.00m);
        appointment.DomainEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_StartTimeAfterEndTime_ThrowsArgumentException()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = DateTime.UtcNow.AddDays(1).AddHours(-1);

        var act = () => new Appointment(
            "BOK-TEST", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            start, end, 100m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Start time must be before end time.*");
    }

    [Fact]
    public void Confirm_FromPending_TransitionsToConfirmed()
    {
        var appointment = CreatePendingAppointment();

        appointment.Confirm();

        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.DomainEvents.Should().Contain(e => e.GetType().Name == "AppointmentConfirmedEvent");
    }

    [Fact]
    public void Confirm_FromConfirmed_ThrowsInvalidOperationException()
    {
        var appointment = CreatePendingAppointment();
        appointment.Confirm();

        var act = () => appointment.Confirm();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot confirm*Confirmed*");
    }

    [Fact]
    public void Confirm_FromCancelled_ThrowsInvalidOperationException()
    {
        var appointment = CreatePendingAppointment();
        appointment.Cancel("Changed mind");

        var act = () => appointment.Confirm();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Start_FromConfirmed_TransitionsToInProgress()
    {
        var appointment = CreatePendingAppointment();
        appointment.Confirm();

        appointment.Start();

        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public void Start_FromPending_ThrowsInvalidOperationException()
    {
        var appointment = CreatePendingAppointment();

        var act = () => appointment.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot start*Pending*");
    }

    [Fact]
    public void Complete_FromInProgress_TransitionsToCompleted()
    {
        var appointment = CreatePendingAppointment();
        appointment.Confirm();
        appointment.Start();

        appointment.Complete();

        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Complete_FromPending_ThrowsInvalidOperationException()
    {
        var appointment = CreatePendingAppointment();

        var act = () => appointment.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_FromPending_TransitionsToCancelled()
    {
        var appointment = CreatePendingAppointment();

        appointment.Cancel("Schedule conflict");

        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancellationReason.Should().Be("Schedule conflict");
    }

    [Fact]
    public void Cancel_FromConfirmed_TransitionsToCancelled()
    {
        var appointment = CreatePendingAppointment();
        appointment.Confirm();

        appointment.Cancel();

        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromCompleted_ThrowsInvalidOperationException()
    {
        var appointment = CreatePendingAppointment();
        appointment.Confirm();
        appointment.Start();
        appointment.Complete();

        var act = () => appointment.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot cancel*Completed*");
    }

    [Fact]
    public void Cancel_FromCancelled_ThrowsInvalidOperationException()
    {
        var appointment = CreatePendingAppointment();
        appointment.Cancel();

        var act = () => appointment.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot cancel*Cancelled*");
    }

    [Fact]
    public void MarkNoShow_FromConfirmed_TransitionsToNoShow()
    {
        var appointment = CreatePendingAppointment();
        appointment.Confirm();

        appointment.MarkNoShow();

        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Fact]
    public void MarkNoShow_FromPending_ThrowsInvalidOperationException()
    {
        var appointment = CreatePendingAppointment();

        var act = () => appointment.MarkNoShow();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkNoShow_FromCompleted_ThrowsInvalidOperationException()
    {
        var appointment = CreatePendingAppointment();
        appointment.Confirm();
        appointment.Start();
        appointment.Complete();

        var act = () => appointment.MarkNoShow();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reschedule_FromPending_CreatesNewAppointmentAndLinksSource()
    {
        var appointment = CreatePendingAppointment();
        var newStart = DateTime.UtcNow.AddDays(2);
        var newEnd = DateTime.UtcNow.AddDays(2).AddHours(1);

        var newAppointment = appointment.Reschedule(newStart, newEnd);

        appointment.Status.Should().Be(AppointmentStatus.Rescheduled);
        newAppointment.Status.Should().Be(AppointmentStatus.Pending);
        newAppointment.RescheduledFromId.Should().Be(appointment.Id);
        newAppointment.BookingReference.Should().Be("BOK-TEST01-R");
    }

    [Fact]
    public void FullLifecycle_FollowsExpectedTransitions()
    {
        var appointment = CreatePendingAppointment();

        appointment.Status.Should().Be(AppointmentStatus.Pending);
        appointment.Confirm();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.Start();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
        appointment.Complete();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void SetTimeRange_EndBeforeStart_ThrowsArgumentException()
    {
        var appointment = CreatePendingAppointment();

        var act = () => appointment.SetTimeRange(
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(-1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetNotes_TrimsWhitespace()
    {
        var appointment = CreatePendingAppointment();
        appointment.SetNotes("  Please arrive early  ");

        appointment.CustomerNotes.Should().Be("Please arrive early");
    }

    [Fact]
    public void MarkNotified_SetsFlag()
    {
        var appointment = CreatePendingAppointment();

        appointment.MarkNotified();

        appointment.IsCustomerNotified.Should().BeTrue();
    }

    [Fact]
    public void AppointmentLog_IsCreatedOnConstruction()
    {
        var appointment = CreatePendingAppointment();

        appointment.Logs.Should().HaveCount(1);
        appointment.Logs.First().ToStatus.Should().Be(AppointmentStatus.Pending);
        appointment.Logs.First().Reason.Should().Be("Appointment created");
    }

    [Fact]
    public void Cancel_FromCompleted_ThrowsInvalidOperation()
    {
        var appointment = CreatePendingAppointment();
        appointment.Confirm();
        appointment.Start();
        appointment.Complete();

        var act = () => appointment.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_FromCancelled_ThrowsInvalidOperation()
    {
        var appointment = CreatePendingAppointment();
        appointment.Cancel();

        var act = () => appointment.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }
}
