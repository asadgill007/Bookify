using Bookify.Domain.ValueObjects;
using FluentAssertions;

namespace Bookify.Domain.Tests;

public class ValueObjectTests
{
    [Fact]
    public void Money_SameAmountAndCurrency_AreEqual()
    {
        var m1 = Money.Create(100.00m, "USD");
        var m2 = Money.Create(100.00m, "USD");

        m1.Should().Be(m2);
        (m1 == m2).Should().BeTrue();
        (m1 != m2).Should().BeFalse();
    }

    [Fact]
    public void Money_DifferentAmount_AreNotEqual()
    {
        var m1 = Money.Create(100.00m, "USD");
        var m2 = Money.Create(200.00m, "USD");

        m1.Should().NotBe(m2);
    }

    [Fact]
    public void Money_DifferentCurrency_AreNotEqual()
    {
        var m1 = Money.Create(100.00m, "USD");
        var m2 = Money.Create(100.00m, "EUR");

        m1.Should().NotBe(m2);
    }

    [Fact]
    public void Money_NegativeAmount_Throws()
    {
        var act = () => Money.Create(-50m, "USD");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_ValidEmail_CreatesSuccessfully()
    {
        var email = Email.Create("test@example.com");

        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Email_InvalidEmail_Throws()
    {
        var act = () => Email.Create("not-an-email");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public void Email_EmptyOrWhitespace_Throws(string? value)
    {
        var act = () => Email.Create(value!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_SameValue_AreEqual()
    {
        var e1 = Email.Create("user@example.com");
        var e2 = Email.Create("user@example.com");

        e1.Should().Be(e2);
    }

    [Fact]
    public void PhoneNumber_ValidNumber_CreatesSuccessfully()
    {
        var phone = PhoneNumber.Create("+1234567890");
        phone.Value.Should().Be("+1234567890");
    }

    [Fact]
    public void PhoneNumber_Invalid_Throws()
    {
        var act = () => PhoneNumber.Create("abc");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Address_SameValues_AreEqual()
    {
        var a1 = Address.Create("123 Main St", null, "NYC", "NY", "10001", "USA");
        var a2 = Address.Create("123 Main St", null, "NYC", "NY", "10001", "USA");

        a1.Should().Be(a2);
    }

    [Fact]
    public void Address_DifferentCity_AreNotEqual()
    {
        var a1 = Address.Create("123 Main St", null, "NYC", "NY", "10001", "USA");
        var a2 = Address.Create("123 Main St", null, "LA", "CA", "10001", "USA");

        a1.Should().NotBe(a2);
    }

    [Fact]
    public void GeoLocation_SameCoordinates_AreEqual()
    {
        var g1 = GeoLocation.Create(40.7128, -74.0060);
        var g2 = GeoLocation.Create(40.7128, -74.0060);

        g1.Should().Be(g2);
    }

    [Fact]
    public void GeoLocation_DifferentCoordinates_AreNotEqual()
    {
        var g1 = GeoLocation.Create(40.7128, -74.0060);
        var g2 = GeoLocation.Create(34.0522, -118.2437);

        g1.Should().NotBe(g2);
    }

    [Fact]
    public void TimeRange_ValidRange_CreatesSuccessfully()
    {
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(17, 0);

        var range = TimeRange.Create(start, end);

        range.StartTime.Should().Be(start);
        range.EndTime.Should().Be(end);
    }

    [Fact]
    public void TimeRange_EndBeforeStart_Throws()
    {
        var start = new TimeOnly(17, 0);
        var end = new TimeOnly(9, 0);

        var act = () => TimeRange.Create(start, end);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TimeRange_SameStartAndEnd_Throws()
    {
        var time = new TimeOnly(12, 0);

        var act = () => TimeRange.Create(time, time);
        act.Should().Throw<ArgumentException>();
    }
}
