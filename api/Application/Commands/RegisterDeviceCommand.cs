using CasinoRoyale.Api.Domain.Entities;

namespace CasinoRoyale.Api.Application.Commands;

public record RegisterDeviceCommand(
    string Name,
    DeviceType Type) : ICommand<DeviceRegistrationResult>;

public record DeviceRegistrationResult(
    Guid DeviceId,
    string ApiKey);