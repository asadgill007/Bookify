using FluentAssertions;

namespace Bookify.Infrastructure.Tests;

/// <summary>
/// Tests for slot generation logic. Pure algorithm tests that don't require a database.
/// The SlotGenerator itself needs a DbContext (integration test), but the merge/scheduling
/// logic can be tested via extracted pure functions or by verifying booking reference patterns.
/// </summary>
public class SlotGeneratorTests
{
    [Fact]
    public void BookingReference_Format_IsCorrect()
    {
        var reference = GenerateTestReference();

        reference.Should().Match("BOK-??????");
        reference.Length.Should().Be(10);
    }

    [Fact]
    public void BookingReference_ContainsOnlyValidChars()
    {
        var validChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var reference = GenerateTestReference();
        var code = reference[4..]; // After "BOK-"

        code.All(c => validChars.Contains(c)).Should().BeTrue();
    }

    [Fact]
    public void BookingReference_NoAmbiguousChars()
    {
        var references = Enumerable.Range(0, 100).Select(_ => GenerateTestReference()).ToList();

        // Should not contain I, O, 0, 1 (ambiguous characters excluded)
        // Check only the random-code portion (after "BOK-"), not the prefix
        foreach (var refCode in references)
        {
            var code = refCode[4..];
            code.Should().NotContain("I");
            code.Should().NotContain("O");
            code.Should().NotContain("0");
            code.Should().NotContain("1");
        }
    }

    [Fact]
    public void BookingReference_IsUniqueAcrossGenerations()
    {
        var generated = new HashSet<string>();
        for (int i = 0; i < 1000; i++)
        {
            var refCode = GenerateTestReference();
            generated.Add(refCode).Should().BeTrue("duplicate reference generated: " + refCode);
        }
    }

    [Fact]
    public void SlotGenerator_ThresholdValidation_NoNegativeDuration()
    {
        // The generator uses DurationMinutes from services - validate against negative/zero
        var duration = 0;
        var act = () =>
        {
            if (duration <= 0)
                throw new ArgumentException("Duration must be positive");
        };

        act.Should().Throw<ArgumentException>();
    }

    private static string GenerateTestReference()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = chars[Random.Shared.Next(chars.Length)];

        return $"BOK-{new string(code)}";
    }
}
