using CasinoRoyale.Api.Domain.Entities;
using NodaTime;

namespace CasinoRoyale.Api.Domain.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    Instant Timestamp { get; }
    Guid LocationId { get; }
}

public record DailyMenuCreatedEvent(Guid MenuId, LocalDate Date, Guid LocationId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record DailyMenuDisabledEvent(Guid MenuId, LocalDate Date, Guid LocationId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record DailyMenuEnabledEvent(Guid MenuId, LocalDate Date, Guid LocationId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record MenuItemAddedEvent(Guid MenuId, MenuItem MenuItem, Guid LocationId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record MenuItemRemovedEvent(Guid MenuId, LocalDate Date, Guid MenuItemId, Guid LocationId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record DeviceRegisteredEvent(
    Guid DeviceId,
    string Name,
    DeviceType Type,
    string ApiKey,
    Guid LocationId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}