using CustomerApi.Common;
using CustomerApi.DTOs;
using CustomerApi.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IValidator<CreateCustomerDto> _createValidator;
        private readonly IValidator<UpdateCustomerDto> _updateValidator;
        private readonly IValidator<CustomerQueryParams> _queryParamsValidator;


        // Validators are injected directly (rather than relying only on automatic pipeline
        // validation) so we control exactly when/how validation errors get formatted.
        public CustomerController(
            ICustomerService customerService,
            IValidator<CreateCustomerDto> createValidator,
            IValidator<UpdateCustomerDto> updateValidator,
            IValidator<CustomerQueryParams> queryParamsValidator)
        {
            _customerService = customerService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _queryParamsValidator = queryParamsValidator;

        }

        // GET /api/Customer — returns every customer wrapped in the uniform response.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(ApiResponse<List<CustomerResponseDto>>.SuccessResponse(customers));
        }

        // GET /api/Customer/{id} — returns one customer, or 404 if not found.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound(ApiResponse<CustomerResponseDto>.FailResponse("Customer not found"));

            return Ok(ApiResponse<CustomerResponseDto>.SuccessResponse(customer));
        }

        // POST /api/Customer — validates via FluentValidation, then creates.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CustomerResponseDto>.FailResponse("Validation failed", errors));
            }

            var created = await _customerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<CustomerResponseDto>.SuccessResponse(created, "Customer created"));
        }

        // PUT /api/Customer/{id} — validates via FluentValidation, then updates.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CustomerResponseDto>.FailResponse("Validation failed", errors));
            }

            var updated = await _customerService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(ApiResponse<CustomerResponseDto>.FailResponse("Customer not found"));

            return Ok(ApiResponse<CustomerResponseDto>.SuccessResponse(updated, "Customer updated"));
        }

        // DELETE /api/Customer/{id} — deletes if found, 404 otherwise.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _customerService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<object>.FailResponse("Customer not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Customer deleted"));
        }


        #region  NEW APIs

        // PATCH /api/Customer/{id}/soft-delete
        // Separate from DELETE /api/Customer/{id} above — the hard-delete endpoint
        // still exists unchanged; this is an additional, non-destructive option.
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var deleted = await _customerService.SoftDeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<object>.FailResponse("Customer not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Customer soft-deleted"));
        }



        // GET /api/Customer/paged?pageNumber=1&pageSize=20&search=okafor&sortBy=lastName&sortDir=desc
        [HttpGet("paged")]
        public async Task<IActionResult> GetAllPaged([FromQuery] CustomerQueryParams queryParams)
        {
            var validationResult = await _queryParamsValidator.ValidateAsync(queryParams);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(PagedResponse<CustomerResponseDto>.Fail("Invalid query parameters", errors));
            }

            var (items, totalCount) = await _customerService.GetAllPagedAsync(queryParams);
            return Ok(PagedResponse<CustomerResponseDto>.Create(items, totalCount, queryParams.PageNumber, queryParams.PageSize));
        }



        // POST /api/Customer/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateMany([FromBody] BulkCreateCustomerDto request)
        {
            if (request.Customers == null || request.Customers.Count == 0)
                return BadRequest(ApiResponse<BulkCreateResultDto>.FailResponse("No customers provided"));

            var result = await _customerService.CreateManyAsync(request.Customers);

            var message = result.FailureCount == 0
                ? "All customers created successfully"
                : $"{result.SuccessCount} created, {result.FailureCount} failed";

            return Ok(ApiResponse<BulkCreateResultDto>.SuccessResponse(result, message));
        }





        #endregion

    }


}
