using CustomerApi.Common;
using CustomerApi.DTOs.Auth;
using CustomerApi.Services.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.FailResponse("Validation failed", errors));
            }

            var result = await _authService.RegisterAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Registration successful"));
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.FailResponse("Validation failed", errors));
            }

            var result = await _authService.LoginAsync(dto);
            if (result == null)
                // Deliberately vague — never reveal whether it was the email or
                // password that was wrong, that's an account enumeration risk.
                return Unauthorized(ApiResponse<AuthResponseDto>.FailResponse("Invalid email or password"));

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
            if (result == null)
                return Unauthorized(ApiResponse<AuthResponseDto>.FailResponse("Invalid or expired refresh token"));

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Token refreshed"));
        }

        // POST /api/auth/revoke  — logout
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto dto)
        {
            var revoked = await _authService.RevokeTokenAsync(dto.RefreshToken);
            if (!revoked)
                return NotFound(ApiResponse<object>.FailResponse("Token not found or already revoked"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Token revoked"));
        }
    }
}
