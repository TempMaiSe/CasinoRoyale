using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using KurrentDB.Client;
using MediatR;
using System.Text;
using System.Text.Json;

namespace CasinoRoyale.Api.Application.Queries;

public record LocationDto(Guid Id, string Name, string TimeZoneId, bool IsActive);
public record GetLocationsQuery : IQuery<IEnumerable<LocationDto>>;

public class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, IEnumerable<LocationDto>>
{
    private readonly KurrentDBClient _eventStore;

    public GetLocationsQueryHandler(KurrentDBClient eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<IEnumerable<LocationDto>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, LocationDto>();
        var events = _eventStore.ReadAllAsync(
            Direction.Forwards,
            Position.Start,
            cancellationToken: cancellationToken);

        await foreach (var @event in events)
        {
            if (@event.Event.EventType.Equals("LocationCreated", StringComparison.OrdinalIgnoreCase))
            {
                var locationEvent = JsonSerializer.Deserialize<LocationCreatedEvent>(
                    Encoding.UTF8.GetString(@event.Event.Data.Span));
                result[locationEvent.LocationId] = new LocationDto(
                    locationEvent.LocationId, 
                    locationEvent.Name, 
                    locationEvent.TimeZoneId,
                    true);
            }
            else if (@event.Event.EventType.Equals("LocationDeactivated", StringComparison.OrdinalIgnoreCase))
            {
                var locationEvent = JsonSerializer.Deserialize<LocationDeactivatedEvent>(
                    Encoding.UTF8.GetString(@event.Event.Data.Span));
                if (result.TryGetValue(locationEvent.LocationId, out var location))
                {
                    result[locationEvent.LocationId] = location with { IsActive = false };
                }
            }
            else if (@event.Event.EventType.Equals("LocationActivated", StringComparison.OrdinalIgnoreCase))
            {
                var locationEvent = JsonSerializer.Deserialize<LocationActivatedEvent>(
                    Encoding.UTF8.GetString(@event.Event.Data.Span));
                if (result.TryGetValue(locationEvent.LocationId, out var location))
                {
                    result[locationEvent.LocationId] = location with { IsActive = true };
                }
            }
        }

        return result.Values;
    }
}