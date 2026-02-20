using Mapster;
using Relatio.Customers.Application.DTOs;
using Relatio.Customers.Domain.Entities;

namespace Relatio.Customers.Application.Mapping;

public sealed class CustomerMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Customer, CustomerDto>()
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.Phone, src => src.Phone != null ? src.Phone.Value : null)
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
