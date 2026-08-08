using CustomerApi.DTOs;
using FluentValidation;

namespace CustomerApi.Validators
{
    // Same rule set as CreateCustomerDtoValidator, kept as its own class in case
    // update rules need to diverge later (e.g. allowing partial updates).
    public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Invalid email format");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number format");
        }
    }
}
