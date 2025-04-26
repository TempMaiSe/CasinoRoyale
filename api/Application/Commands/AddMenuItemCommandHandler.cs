using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using KurrentDB.Client;
using MediatR;
using System.Text;
using System.Text.Json;

namespace CasinoRoyale.Api.Application.Commands;

public class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, Guid>
{
    private readonly KurrentDBClient _eventStore;

    public AddMenuItemCommandHandler(KurrentDBClient eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Guid> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
    {
        // Verify daily menu exists and belongs to the specified location
        var dailyMenuStream = $"dailymenu-{request.LocationId}-{request.DailyMenuId}";
        var dailyMenuExists = await _eventStore.ReadStreamAsync(
            Direction.Backwards,
            dailyMenuStream,
            StreamPosition.End,
            1,
            cancellationToken: cancellationToken)
            .AnyAsync(cancellationToken);

        if (!dailyMenuExists)
        {
            throw new InvalidOperationException($"Daily menu {request.DailyMenuId} not found for location {request.LocationId}");
        }

        var menuItem = new MenuItem(
            request.Name,
            request.Description,
            request.EmployeePrice,
            request.ExternalPrice,
            request.Allergens,
            request.Type,
            request.IsSpecialOffer,
            request.SpecialOfferDay);

        var @event = new MenuItemAddedEvent(request.DailyMenuId, menuItem, request.LocationId);
        var eventData = new EventData(
            Uuid.NewUuid(),
            "MenuItemAdded",
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)));

        await _eventStore.AppendToStreamAsync(
            dailyMenuStream,
            StreamState.Any,
            new[] { eventData },
            cancellationToken: cancellationToken);

        return menuItem.Id;
    }
}