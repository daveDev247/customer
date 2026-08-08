using CustomerApi.DTOs;
using FluentValidation;

namespace CustomerApi.Validators
{
    // FluentValidation rules for creating a customer.
    // Same rules as the old Data Annotations, expressed as a fluent chain instead of attributes.
    public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
    {
        public CreateCustomerDtoValidator()
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
