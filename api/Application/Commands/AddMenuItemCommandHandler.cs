using CasinoRoyale.Api.Domain.Entities;
using KurrentDB.Client;
using MediatR;

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
        var menuItem = new MenuItem(
            request.Name,
            request.Description,
            request.EmployeePrice,
            request.ExternalPrice,
            request.Allergens,
            request.Type,
            request.IsSpecialOffer,
            request.SpecialOfferDay);

        var @event = new MenuItemAddedEvent(request.DailyMenuId, menuItem);
        var eventData = new EventData(
            Uuid.NewUuid(),
            "MenuItemAdded",
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)));

        await _eventStore.AppendToStreamAsync(
            $"dailymenu-{request.DailyMenuId}",
            StreamState.Any,
            new[] { eventData },
            cancellationToken: cancellationToken);

        return menuItem.Id;
    }
}