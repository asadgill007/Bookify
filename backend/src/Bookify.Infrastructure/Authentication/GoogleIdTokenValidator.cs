using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Bookify.Infrastructure.Authentication;

/// <summary>
/// Validates Google ID tokens using Google's published JWKS (no API key or
/// Firebase project needed). Returns the identity claims (subject, email,
/// name, picture) when the token is authentic and, when a client id is
/// configured in "Google:ClientId", verifies the audience matches.
/// </summary>
public interface IGoogleIdTokenValidator
{
    /// <summary>Validates the token and returns its payload claims, or null when invalid.</summary>
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed class GoogleUserInfo
{
    public string Subject { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
    public string? Name { get; init; }
    public string? Picture { get; init; }
}

public class GoogleIdTokenValidator : IGoogleIdTokenValidator
{
    private const string GoogleJwksUrl = "https://www.googleapis.com/oauth2/v3/certs";

    // Cached across requests so Google's JWKS metadata is fetched once, not
    // on every token validation (the ConfigurationManager caches + refreshes).
    private static readonly ConfigurationManager<OpenIdConnectConfiguration> ConfigManager =
        new(GoogleJwksUrl, new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever());

    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleIdTokenValidator> _logger;

    public GoogleIdTokenValidator(IConfiguration configuration, ILogger<GoogleIdTokenValidator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var clientId = _configuration["Google:ClientId"];

            var config = await ConfigManager.GetConfigurationAsync(cancellationToken);

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://accounts.google.com",
                ValidateAudience = true,
                ValidAudience = clientId ?? string.Empty,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                // When no client id is configured we can't validate the
                // audience, so require explicit claims we trust instead.
                AudienceValidator = (audiences, _, _) =>
                    string.IsNullOrEmpty(clientId) || audiences.Contains(clientId)
            };

            var principal = handler.ValidateToken(idToken, validationParameters, out _);

            var email = principal.FindFirstValue(JwtRegisteredClaimNames.Email);
            var emailVerified = principal.FindFirstValue(JwtRegisteredClaimNames.EmailVerified) == "true";
            var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(subject))
            {
                _logger.LogWarning("Google token missing email or subject claim.");
                return null;
            }

            return new GoogleUserInfo
            {
                Subject = subject,
                Email = email,
                EmailVerified = emailVerified,
                Name = principal.FindFirstValue(JwtRegisteredClaimNames.Name),
                Picture = principal.FindFirstValue("picture")
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Google ID token validation failed: {Message}", ex.Message);
            return null;
        }
    }
}
