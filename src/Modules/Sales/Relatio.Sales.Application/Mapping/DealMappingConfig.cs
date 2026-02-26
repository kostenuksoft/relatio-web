using Mapster;
using Relatio.Sales.Application.DTOs;
using Relatio.Sales.Domain.Entities;

namespace Relatio.Sales.Application.Mapping;

public sealed class DealMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Deal, DealDto>()
            .Map(dest => dest.Stage, src => src.Stage.ToString());
    }
}
