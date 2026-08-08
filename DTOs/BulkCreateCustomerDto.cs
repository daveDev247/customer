namespace CustomerApi.DTOs
{
    public class BulkCreateCustomerDto
    {
        public List<CreateCustomerDto> Customers { get; set; } = new();
    }
}
