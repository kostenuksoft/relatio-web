namespace Relatio.Shared.Models;

public sealed class CursorResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public Guid? NextCursor { get; init; }
    public bool HasNextPage => NextCursor.HasValue;
}
