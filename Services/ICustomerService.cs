using CustomerApi.DTOs;

namespace CustomerApi.Services
{
    // Contract the controller depends on. Keeps the controller unaware of EF Core / AutoMapper details.
    public interface ICustomerService
    {
        Task<List<CustomerResponseDto>> GetAllAsync();
        Task<CustomerResponseDto?> GetByIdAsync(int id);
        Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto);
        Task<CustomerResponseDto?> UpdateAsync(int id, UpdateCustomerDto dto);
        Task<bool> DeleteAsync(int id);

        Task<bool> SoftDeleteAsync(int id);

        // New method signature added to the interface (existing methods unchanged).
        Task<(List<CustomerResponseDto> Items, int TotalCount)> GetAllPagedAsync(CustomerQueryParams queryParams);

        Task<BulkCreateResultDto> CreateManyAsync(List<CreateCustomerDto> dtos);

    }
}
