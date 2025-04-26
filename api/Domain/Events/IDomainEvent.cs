namespace CasinoRoyale.Api.Domain.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime Timestamp { get; }
}

public record DailyMenuCreatedEvent(Guid MenuId, DateOnly Date) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

public record DailyMenuDisabledEvent(Guid MenuId, DateOnly Date) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

public record DailyMenuEnabledEvent(Guid MenuId, DateOnly Date) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

public record MenuItemAddedEvent(Guid MenuId, MenuItem MenuItem) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

public record MenuItemRemovedEvent(Guid MenuId, DateOnly Date, Guid MenuItemId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

public record DeviceRegisteredEvent(
    Guid DeviceId,
    string Name,
    DeviceType Type,
    string ApiKey) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}