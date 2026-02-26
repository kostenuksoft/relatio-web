namespace Relatio.Shared.Abstractions;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
}
