using System.ComponentModel.DataAnnotations;

namespace CustomerApi.DTOs
{
    // Shape of data accepted when creating a customer.
    // No Data Annotations here anymore — validation now lives in CreateCustomerDtoValidator (FluentValidation).
    public class CreateCustomerDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
