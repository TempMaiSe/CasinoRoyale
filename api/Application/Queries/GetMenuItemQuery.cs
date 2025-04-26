using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using KurrentDB.Client;
using MediatR;
using System.Text;
using System.Text.Json;

namespace CasinoRoyale.Api.Application.Queries;

public record GetMenuItemQuery(Guid Id) : IQuery<MenuItem?>;

public class GetMenuItemQueryHandler : IRequestHandler<GetMenuItemQuery, MenuItem?>
{
    private readonly KurrentDBClient _eventStore;

    public GetMenuItemQueryHandler(KurrentDBClient eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<MenuItem?> Handle(GetMenuItemQuery request, CancellationToken cancellationToken)
    {
        var events = _eventStore.ReadAllAsync(
            Direction.Forwards,
            Position.Start,
            cancellationToken: cancellationToken);

        await foreach (var @event in events)
        {
            if (!@event.Event.EventType.Equals("MenuItemAdded", StringComparison.OrdinalIgnoreCase))
                continue;

            var menuEventData = JsonSerializer.Deserialize<MenuItemAddedEvent>(
                Encoding.UTF8.GetString(@event.Event.Data.Span));

            if (menuEventData?.MenuItem.Id == request.Id)
                return menuEventData.MenuItem;
        }

        return null;
    }
}