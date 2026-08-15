using CustomerApi.Common;
using CustomerApi.Data;
using CustomerApi.DTOs.Auth;
using CustomerApi.Models;
using CustomerApi.Services.Interface;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            IJwtTokenService tokenService,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        // Hashes the password (never stores plaintext), saves the user,
        // then immediately issues a token pair so the caller is logged in right away.
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                // BCrypt.HashPassword salts and hashes in one call — the salt is
                // embedded in the resulting hash string, so nothing extra to store.
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {UserId}", user.Id);
            return await IssueTokensAsync(user);
        }

        // Verifies the password against the stored hash. Returns null (not an
        // exception) on bad credentials — the controller turns that into a 401.
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return null;

            // BCrypt.Verify re-hashes the input with the stored salt and compares —
            // this is why we never need to "decrypt" the stored hash.
            var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!passwordValid) return null;

            _logger.LogInformation("User {UserId} logged in", user.Id);
            return await IssueTokensAsync(user);
        }

        // Validates the refresh token is still active, revokes it, and issues a brand
        // new pair — this "rotation" means a stolen refresh token only works once
        // before the legitimate client's next refresh invalidates it.
        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || !storedToken.IsActive || storedToken.User == null)
            {
                _logger.LogWarning("Refresh attempted with invalid or expired token");
                return null;
            }

            // Revoke the old token immediately — it cannot be used again after this point.
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token rotated for user {UserId}", storedToken.UserId);
            return await IssueTokensAsync(storedToken.User);
        }

        // Explicit logout — revokes a refresh token so it can never be used again,
        // even though it hasn't expired yet.
        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (storedToken == null || !storedToken.IsActive) return false;

            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked for user {UserId}", storedToken.UserId);
            return true;
        }

        // Shared by Register/Login/Refresh — generates both tokens, persists the
        // refresh token row, and returns the shape the client needs.
        private async Task<AuthResponseDto> IssueTokensAsync(User user)
        {
            var (accessToken, accessExpiresAt) = _tokenService.GenerateAccessToken(user);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiresAt = refreshExpiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiresAt = refreshExpiresAt
            };
        }
    }
}
