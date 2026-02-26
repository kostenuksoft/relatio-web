using Mapster;
using Relatio.Contacts.Application.DTOs;
using Relatio.Contacts.Domain.Entities;

namespace Relatio.Contacts.Application.Mapping;

public sealed class ContactMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Contact, ContactDto>()
            .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
            .Map(dest => dest.Email, src => src.Email != null ? src.Email.Value : null)
            .Map(dest => dest.Phone, src => src.Phone != null ? src.Phone.Value : null);
    }
}
