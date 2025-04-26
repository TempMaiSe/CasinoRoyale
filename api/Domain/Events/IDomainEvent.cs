using CasinoRoyale.Api.Domain.Entities;
using NodaTime;

namespace CasinoRoyale.Api.Domain.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    Instant Timestamp { get; }
}

public record DailyMenuCreatedEvent(Guid MenuId, LocalDate Date) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record DailyMenuDisabledEvent(Guid MenuId, LocalDate Date) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record DailyMenuEnabledEvent(Guid MenuId, LocalDate Date) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record MenuItemAddedEvent(Guid MenuId, MenuItem MenuItem) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record MenuItemRemovedEvent(Guid MenuId, LocalDate Date, Guid MenuItemId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}

public record DeviceRegisteredEvent(
    Guid DeviceId,
    string Name,
    DeviceType Type,
    string ApiKey) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}