using CustomerApi.DTOs;
using FluentValidation;

namespace CustomerApi.Validators
{
    // Same FluentValidation pattern as CreateCustomerDtoValidator — query params
    // get the same treatment as request bodies now, rather than reverting to
    // manual if-checks just because this data comes from the query string instead of JSON.
    public class CustomerQueryParamsValidator : AbstractValidator<CustomerQueryParams>
    {
        public CustomerQueryParamsValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("pageNumber must be 1 or greater");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("pageSize must be between 1 and 100");

            RuleFor(x => x.SortDir)
                .Must(dir => string.IsNullOrWhiteSpace(dir) ||
                             string.Equals(dir, "asc", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("sortDir must be 'asc' or 'desc'");
        }
    }
}
