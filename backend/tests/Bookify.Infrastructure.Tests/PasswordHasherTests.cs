using Bookify.Infrastructure.Authentication;
using FluentAssertions;

namespace Bookify.Infrastructure.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ReturnsNonNullHash()
    {
        var hash = _sut.Hash("SecureP@ss1");

        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_SamePassword_ReturnsDifferentHashEachTime()
    {
        var hash1 = _sut.Hash("SecureP@ss1");
        var hash2 = _sut.Hash("SecureP@ss1");

        hash1.Should().NotBe(hash2); // PBKDF2 uses random salt
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("SecureP@ss1");

        var result = _sut.Verify("SecureP@ss1", hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("SecureP@ss1");

        var result = _sut.Verify("WrongPassword", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("SecureP@ss1");

        var result = _sut.Verify("", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_EmptyPassword_ReturnsHash()
    {
        var hash = _sut.Hash("");

        hash.Should().NotBeNullOrWhiteSpace();
    }
}
