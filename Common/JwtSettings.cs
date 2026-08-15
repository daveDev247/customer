namespace CustomerApi.Common
{
    // Strongly-typed binding target for the "JwtSettings" section in appsettings.json.
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
    }
}
