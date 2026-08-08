using System.ComponentModel.DataAnnotations;

namespace CustomerApi.DTOs
{
    
        // Shape of data accepted when updating a customer. Validated by UpdateCustomerDtoValidator.
        public class UpdateCustomerDto
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }
    
}
