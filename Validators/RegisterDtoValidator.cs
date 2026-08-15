using Microsoft.EntityFrameworkCore;
using CustomerApi.Data;
using CustomerApi.DTOs.Auth;
using FluentValidation;


namespace CustomerApi.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator(AppDbContext context)
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MustAsync(async (username, ct) => !await context.Users.AnyAsync(u => u.Username == username, ct))
                .WithMessage("Username is already taken");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Invalid email format")
                .MustAsync(async (email, ct) => !await context.Users.AnyAsync(u => u.Email == email, ct))
                .WithMessage("Email is already registered");

            // Basic password strength rule — worth discussing with the junior dev
            // that this is a minimum bar, not a substitute for a real password policy.
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters");
        }
    }
}
