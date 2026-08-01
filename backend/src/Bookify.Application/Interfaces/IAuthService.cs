using Bookify.Application.Common;
using Bookify.Application.DTOs.Auth;

namespace Bookify.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Provider-role user account for staff with a temporary password.
    /// Used by business owners when adding staff members.
    /// </summary>
    Task<Result<Domain.Entities.User>> RegisterStaffAsync(
        string firstName,
        string lastName,
        string email,
        string? avatarUrl,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates with a Google ID token. If the user already exists
    /// (matched by Google subject or email) they are signed in; otherwise a
    /// new account is created (Customer by default — users can upgrade to a
    /// provider/business owner later).
    /// </summary>
    Task<Result<AuthResponse>> LoginWithGoogleAsync(
        string idToken,
        string? accountType,
        CancellationToken cancellationToken = default);
}
