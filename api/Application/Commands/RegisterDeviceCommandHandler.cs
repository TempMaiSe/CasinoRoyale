using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using KurrentDB.Client;
using MediatR;
using System.Text.Json;
using System.Text;

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
        var device = new Device(request.Name, request.Type);

        var @event = new DeviceRegisteredEvent(device.Id, device.Name, device.Type, device.ApiKey);
        var eventData = new EventData(
            Uuid.NewUuid(),
            "DeviceRegistered",
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)));

        await _eventStore.AppendToStreamAsync(
            $"device-{device.Id}",
            StreamState.NoStream,
            new[] { eventData },
            cancellationToken: cancellationToken);

        return new DeviceRegistrationResult(device.Id, device.ApiKey);
    }
}