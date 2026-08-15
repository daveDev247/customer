using CustomerApi.Common;
using CustomerApi.Models;
using CustomerApi.Services.Interface;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CustomerApi.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;

        // IOptions<JwtSettings> is how we read the strongly-typed config bound
        // from appsettings.json, injected via DI rather than reading raw config directly.
        public JwtTokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

            // Claims are the pieces of identity info embedded (readably, not secretly)
            // inside the token — anything sensitive should NOT go here, since JWTs are
            // signed, not encrypted, and can be decoded by anyone.
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        public string GenerateRefreshToken()
        {
            // 64 random bytes, base64-encoded — unguessable, and carries no embedded
            // data of its own (unlike the access token, it's just a lookup key).
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
