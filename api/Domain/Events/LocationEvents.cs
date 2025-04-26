using NodaTime;

namespace CasinoRoyale.Api.Domain.Events;

public record LocationCreatedEvent(
    Guid LocationId,
    string Name,
    string TimeZoneId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
    Guid IDomainEvent.LocationId => LocationId;
}

public record LocationDeactivatedEvent(Guid LocationId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
    Guid IDomainEvent.LocationId => LocationId;
}

public record LocationActivatedEvent(Guid LocationId) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateSequential();
    public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
    Guid IDomainEvent.LocationId => LocationId;
}