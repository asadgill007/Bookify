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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
            return Result<AuthResponse>.Failure("Email is already registered.", "EMAIL_EXISTS");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            UserRole.Customer,
            request.PhoneNumber);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        var userPreference = new UserPreference(user.Id);
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

        _logger.LogInformation("User registered: {Email} with ID {UserId}", user.Email, user.Id);

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

    public Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        // In production, send email with reset link
        _logger.LogInformation("Password reset requested for: {Email}", email);
        return Task.FromResult(Result.Success());
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "USER_NOT_FOUND");

        user.ChangePassword(_passwordHasher.Hash(newPassword));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
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
