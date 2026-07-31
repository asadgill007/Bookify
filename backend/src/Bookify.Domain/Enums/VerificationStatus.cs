namespace Bookify.Domain.Enums;

/// <summary>
/// Lifecycle state of a business listing with respect to admin verification.
/// Newly registered businesses start as <see cref="Pending"/> and only become
/// publicly visible in customer search once an admin <see cref="Approved"/> them.
/// </summary>
public enum VerificationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
