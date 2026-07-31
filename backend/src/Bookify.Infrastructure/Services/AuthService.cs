using System.Security.Cryptography;
using System.Text;
using Bookify.Application.Common;
using Bookify.Application.DTOs.Auth;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Authentication;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

public class AuthService : IAuthService
{
    private const int ResetCodeTtlMinutes = 15;
    private const string ResetCodePrefix = "password-reset:";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        ICacheService cacheService,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
            return Result<AuthResponse>.Failure("Email is already registered.", "EMAIL_EXISTS");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var role = MapAccountTypeToRole(request.AccountType);
        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            role,
            request.PhoneNumber);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.UserPreferences.AddAsync(new UserPreference(user.Id), cancellationToken);

        await _unitOfWork.Notifications.AddAsync(
            new Notification(user.Id, NotificationType.System,
                "Welcome to Bookify!",
                "Thank you for joining. Start exploring premium services near you."),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tokens = await _jwtService.GenerateTokensAsync(user.Id, user.Email, user.Role.ToString(), cancellationToken);

        var refreshToken = new RefreshToken(
            user.Id,
            tokens.RefreshToken,
            Guid.NewGuid().ToString(),
            tokens.RefreshTokenExpiresAt);

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered: {Email} with ID {UserId} as {Role}", user.Email, user.Id, user.Role);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresIn = tokens.ExpiresInSeconds
        });
    }

    public async Task<Result<Domain.Entities.User>> RegisterStaffAsync(
        string firstName,
        string lastName,
        string email,
        string? avatarUrl,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existing = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existing != null)
        {
            if (existing.Role == UserRole.Customer)
                existing.SetRole(UserRole.Provider);
            return Result<Domain.Entities.User>.Success(existing);
        }

        // Generate a temporary password; the staff member can reset it via forgot-password.
        var tempPassword = $"Temp!{Guid.NewGuid():N}"[..20];
        var user = new User(
            firstName,
            lastName,
            normalizedEmail,
            _passwordHasher.Hash(tempPassword),
            UserRole.Provider);
        user.SetAvatar(avatarUrl);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        // Note: no SaveChanges here — the caller (AddBusinessProviderCommandHandler)
        // persists the user together with the Provider row in a single transaction.

        _logger.LogInformation("Staff account created: {Email} with ID {UserId}", user.Email, user.Id);
        return Result<Domain.Entities.User>.Success(user);
    }

    private static UserRole MapAccountTypeToRole(string? accountType)
    {
        return accountType?.Trim().ToLowerInvariant() switch
        {
            "provider" => UserRole.Provider,
            "businessowner" => UserRole.BusinessOwner,
            _ => UserRole.Customer
        };
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("Invalid email or password.", "INVALID_CREDENTIALS");

        user.RecordLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tokens = await _jwtService.GenerateTokensAsync(user.Id, user.Email, user.Role.ToString(), cancellationToken);

        var refreshToken = new RefreshToken(
            user.Id,
            tokens.RefreshToken,
            Guid.NewGuid().ToString(),
            tokens.RefreshTokenExpiresAt);

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User logged in: {Email}", user.Email);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresIn = tokens.ExpiresInSeconds
        });
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = await _jwtService.ValidateTokenAsync(request.AccessToken, cancellationToken);
        if (principal == null)
            return Result<AuthResponse>.Failure("Invalid access token.", "INVALID_TOKEN");

        var userId = Guid.Parse(principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var storedRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (storedRefreshToken == null || !storedRefreshToken.IsActive)
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");

        storedRefreshToken.MarkAsUsed();

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result<AuthResponse>.Failure("User not found.", "USER_NOT_FOUND");

        var tokens = await _jwtService.GenerateTokensAsync(user.Id, user.Email, user.Role.ToString(), cancellationToken);

        var newRefreshToken = new RefreshToken(
            user.Id,
            tokens.RefreshToken,
            Guid.NewGuid().ToString(),
            tokens.RefreshTokenExpiresAt);

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresIn = tokens.ExpiresInSeconds
        });
    }

    public async Task<Result> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken);
        if (storedToken != null)
        {
            storedToken.Revoke();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        // Always return success regardless of whether the account exists to avoid
        // leaking which email addresses are registered (user enumeration prevention).
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);

        if (user is not null)
        {
            var code = GenerateResetCode();
            var codeHash = HashResetCode(code);
            var normalizedEmail = email.Trim().ToLowerInvariant();

            await _cacheService.SetAsync(
                $"{ResetCodePrefix}{normalizedEmail}",
                new PasswordResetCode { CodeHash = codeHash, ExpiresAt = DateTime.UtcNow.AddMinutes(ResetCodeTtlMinutes) },
                new CacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ResetCodeTtlMinutes) },
                cancellationToken);

            // In test mode the email body (including the code) is logged so the flow
            // can be exercised end-to-end without a real SMTP server.
            await _emailService.SendPasswordResetEmailAsync(email, $"{user.FirstName} {user.LastName}", code, cancellationToken);

            _logger.LogInformation("Password reset code issued for: {Email}", email);
        }
        else
        {
            // Simulate work so response timing does not reveal account existence.
            await Task.Delay(100, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var cacheKey = $"{ResetCodePrefix}{normalizedEmail}";
        var entry = await _cacheService.GetAsync<PasswordResetCode>(cacheKey, cancellationToken);
        if (entry is null || entry.ExpiresAt <= DateTime.UtcNow)
            return Result.Failure("The reset code is invalid or has expired. Please request a new one.", "INVALID_RESET_CODE");

        if (!VerifyResetCode(token, entry.CodeHash))
            return Result.Failure("The reset code is invalid or has expired. Please request a new one.", "INVALID_RESET_CODE");

        var user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "USER_NOT_FOUND");

        user.ChangePassword(_passwordHasher.Hash(newPassword));

        // A password change invalidates every outstanding refresh token.
        await _unitOfWork.RefreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Single-use: remove the code so it cannot be reused.
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("Password reset completed for: {Email}", email);
        return Result.Success();
    }

    private static string GenerateResetCode()
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        var value = BitConverter.ToUInt32(bytes) % 1_000_000;
        return value.ToString("D6");
    }

    private static string HashResetCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }

    private static bool VerifyResetCode(string code, string expectedHash)
    {
        var actualHash = HashResetCode(code);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(actualHash),
            Encoding.UTF8.GetBytes(expectedHash));
    }

    private sealed class PasswordResetCode
    {
        public string CodeHash { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "USER_NOT_FOUND");

        if (!_passwordHasher.Verify(currentPassword, user.PasswordHash))
            return Result.Failure("Current password is incorrect.", "INVALID_PASSWORD");

        user.ChangePassword(_passwordHasher.Hash(newPassword));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
