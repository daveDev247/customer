namespace CustomerApi.DTOs.Auth
{
    // Returned on successful register/login/refresh — everything the client
    // needs to make authenticated calls and later refresh the session.
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
