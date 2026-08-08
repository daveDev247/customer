using AutoMapper;
using CustomerApi.Data;
using CustomerApi.DTOs;
using CustomerApi.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;
        private readonly IValidator<CreateCustomerDto> _createValidator; // only needed for bulk create


        public CustomerService(AppDbContext context, IMapper mapper, ILogger<CustomerService> logger, IValidator<CreateCustomerDto> createValidator)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;

        }

        // Fetches every customer and maps the list straight to response DTOs via AutoMapper.
        // No try-catch here anymore for generic exceptions — the global middleware (Step 3) now
        // catches, logs, and formats anything unhandled. We only catch where we'd add real value.
        public async Task<List<CustomerResponseDto>> GetAllAsync()
        {
            var customers = await _context.Customers.AsNoTracking().ToListAsync();
            return _mapper.Map<List<CustomerResponseDto>>(customers);
        }

        // Fetches a single customer by Id, or null if not found (controller turns that into a 404).
        public async Task<CustomerResponseDto?> GetByIdAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            return customer == null ? null : _mapper.Map<CustomerResponseDto>(customer);
        }

        // Maps the incoming DTO to a new Customer entity, saves it, and logs the creation
        // with the new Id for traceability. DbUpdateException is still caught explicitly here
        // because we want domain-specific logging context (which fields, which operation)
        // before it bubbles up to the middleware.
        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = _mapper.Map<Customer>(dto);
            customer.CreatedAt = DateTime.UtcNow;

            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while creating customer with Email {Email}", dto.Email);
                throw;
            }

            _logger.LogInformation("Customer created with Id {CustomerId}", customer.Id);
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        // Finds the tracked entity, applies the DTO's values onto it via AutoMapper (Map(dto, customer)
        // updates the existing object instead of creating a new one), then saves.
        public async Task<CustomerResponseDto?> UpdateAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return null;

            _mapper.Map(dto, customer);
            customer.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while updating customer {CustomerId}", id);
                throw;
            }

            _logger.LogInformation("Customer {CustomerId} updated", id);
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        // Removes the customer if found; returns false so the controller can 404 if not.
        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer {CustomerId} deleted", id);
            return true;
        }



        // Soft delete — separate method and separate endpoint from the existing
        // hard-delete DeleteAsync above. Both remain available side by side.
        public async Task<bool> SoftDeleteAsync(int id)
        {
            // Explicitly excludes already soft-deleted rows since there's no global filter.
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer {CustomerId} soft-deleted", id);
            return true;
        }

        // Applies search, then sorting, then pagination, in that order.
        // Only fetches TotalCount once — after filtering, before paging — so the client
        // can correctly compute TotalPages against the filtered set, not the whole table.
        public async Task<(List<CustomerResponseDto> Items, int TotalCount)> GetAllPagedAsync(CustomerQueryParams queryParams)
        {
            var query = _context.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(search) ||
                    c.LastName.ToLower().Contains(search) ||
                    c.Email.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();

            var isDescending = string.Equals(queryParams.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = queryParams.SortBy?.ToLower() switch
            {
                "firstname" => isDescending ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName),
                "lastname" => isDescending ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName),
                "email" => isDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
                _ => isDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
            };

            var customers = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<CustomerResponseDto>>(customers);
            return (mapped, totalCount);
        }



        // Bulk create — validates each item individually so one bad row doesn't
        // block the rest, then a single SaveChangesAsync for the whole valid batch.
        public async Task<BulkCreateResultDto> CreateManyAsync(List<CreateCustomerDto> dtos)
        {
            var result = new BulkCreateResultDto();
            var customersToAdd = new List<Customer>();

            for (int i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];
                var validationResult = await _createValidator.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    result.Errors.Add(new BulkCreateErrorDto
                    {
                        Index = i,
                        Email = dto.Email,
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    });
                    continue;
                }

                var customer = _mapper.Map<Customer>(dto);
                customer.CreatedAt = DateTime.UtcNow;
                customersToAdd.Add(customer);
            }

            if (customersToAdd.Count > 0)
            {
                _context.Customers.AddRange(customersToAdd);
                await _context.SaveChangesAsync();
                result.Created = _mapper.Map<List<CustomerResponseDto>>(customersToAdd);
            }

            _logger.LogInformation("Bulk create: {SuccessCount} created, {FailureCount} failed",
                result.SuccessCount, result.FailureCount);

            return result;
        }





    }








}
