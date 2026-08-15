using CustomerApi.Models;

namespace CustomerApi.Services.Interface
{
    public interface IJwtTokenService
    {
        // Returns the signed JWT string and its expiry time.
        (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

        // Returns a cryptographically random string — not a JWT, just an opaque token
        // we store and look up server-side.
        string GenerateRefreshToken();
    }
}
