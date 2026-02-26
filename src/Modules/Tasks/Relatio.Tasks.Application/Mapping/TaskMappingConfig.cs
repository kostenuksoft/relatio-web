using Mapster;
using Relatio.Tasks.Application.DTOs;
using Relatio.Tasks.Domain.Entities;

namespace Relatio.Tasks.Application.Mapping;

public sealed class TaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CrmTask, TaskDto>()
            .Map(dest => dest.Priority, src => src.Priority.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
