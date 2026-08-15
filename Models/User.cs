namespace CustomerApi.Models
{
    // Minimal credential store. In a real system this would likely live in
    // Identity or a dedicated auth service — kept simple here for teaching.
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Never store plaintext passwords — this holds the BCrypt hash only.
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property — one user can have many refresh tokens issued over time
        // (one per login/device), which is what lets us revoke a specific session.
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
