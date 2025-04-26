using CasinoRoyale.Api.Domain.Aggregates;
using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using KurrentDB.Client;
using MediatR;
using NodaTime;
using System.Text;
using System.Text.Json;

namespace CasinoRoyale.Api.Application.Commands;

public class RegisterDeviceCommandHandler : IRequestHandler<RegisterDeviceCommand, DeviceRegistrationResult>
{
    private readonly KurrentDBClient _eventStore;

    public RegisterDeviceCommandHandler(KurrentDBClient eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<DeviceRegistrationResult> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        // First, get the location
        var locationStream = $"location-{request.LocationId}";
        var locationEvent = await _eventStore.ReadStreamAsync(
            Direction.Backwards,
            locationStream,
            StreamPosition.End,
            1,
            cancellationToken: cancellationToken)
            .FirstAsync(cancellationToken);

        var eventData = JsonSerializer.Deserialize<LocationCreatedEvent>(
            Encoding.UTF8.GetString(locationEvent.Event.Data.Span));
        var location = new Location(eventData.Name, DateTimeZoneProviders.Tzdb[eventData.TimeZoneId]);

        var device = new Device(request.Name, request.Type, location);

        var @event = new DeviceRegisteredEvent(device.Id, device.Name, device.Type, device.ApiKey, location.Id);
        var deviceEventData = new EventData(
            Uuid.NewUuid(),
            "DeviceRegistered",
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)));

        await _eventStore.AppendToStreamAsync(
            $"device-{device.Id}",
            StreamState.NoStream,
            new[] { deviceEventData },
            cancellationToken: cancellationToken);

        return new DeviceRegistrationResult(device.Id, device.ApiKey);
    }
}