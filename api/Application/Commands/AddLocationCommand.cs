using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using KurrentDB.Client;
using MediatR;
using System.Text;
using System.Text.Json;

namespace CasinoRoyale.Api.Application.Commands;

public record AddLocationCommand(string Name, string TimeZoneId) : ICommand<Guid>;

public class AddLocationCommandHandler : IRequestHandler<AddLocationCommand, Guid>
{
    private readonly KurrentDBClient _eventStore;

    public AddLocationCommandHandler(KurrentDBClient eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Guid> Handle(AddLocationCommand request, CancellationToken cancellationToken)
    {
        var locationId = Guid.CreateVersion7();
        var @event = new LocationCreatedEvent(locationId, request.Name, request.TimeZoneId);
        var eventData = new EventData(
            Uuid.NewUuid(),
            "LocationCreated",
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)));

        await _eventStore.AppendToStreamAsync(
            $"location-{locationId}",
            StreamState.NoStream,
            new[] { eventData },
            cancellationToken: cancellationToken);

        return locationId;
    }
}