using AutoMapper;
using CustomerApi.DTOs;
using CustomerApi.Models;

namespace CustomerApi.Common
{
    // Central place AutoMapper reads to know how to convert between DTOs and the Customer model.
    // Replaces the inline object-literal mapping we wrote by hand in the previous stage.
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Model -> Response DTO (used when returning data to the client)
            CreateMap<Customer, CustomerResponseDto>().ReverseMap();

            // Create DTO -> Model (used when inserting a new row)
            CreateMap<CreateCustomerDto, Customer>().ReverseMap();

            // Update DTO -> Model (used when applying changes onto an existing tracked entity)
            CreateMap<UpdateCustomerDto, Customer>().ReverseMap();
        }
    }
}
