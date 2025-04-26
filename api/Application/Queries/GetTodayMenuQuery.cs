using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using KurrentDB.Client;
using MediatR;
using NodaTime;
using System.Text;
using System.Text.Json;

namespace CasinoRoyale.Api.Application.Queries;

public record GetTodayMenuQuery(Guid LocationId) : IQuery<IEnumerable<MenuItem>>;

public class GetTodayMenuQueryHandler : IRequestHandler<GetTodayMenuQuery, IEnumerable<MenuItem>>
{
    private readonly KurrentDBClient _eventStore;
    private readonly IClock _clock;

    public GetTodayMenuQueryHandler(KurrentDBClient eventStore, IClock clock)
    {
        _eventStore = eventStore;
        _clock = clock;
    }

    public async Task<IEnumerable<MenuItem>> Handle(GetTodayMenuQuery request, CancellationToken cancellationToken)
    {
        // First get the location to determine the timezone
        var locationStream = $"location-{request.LocationId}";
        var locationEvent = await _eventStore.ReadStreamAsync(
            Direction.Backwards,
            locationStream,
            StreamPosition.End,
            1,
            cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (locationEvent == null)
        {
            throw new InvalidOperationException($"Location {request.LocationId} not found");
        }

        var eventData = JsonSerializer.Deserialize<LocationCreatedEvent>(
            Encoding.UTF8.GetString(locationEvent.Event.Data.Span));
        var location = new Location(eventData.Name, DateTimeZoneProviders.Tzdb[eventData.TimeZoneId]);

        var today = location.GetCurrentDate(_clock);
        var streamName = $"dailymenu-{request.LocationId}-{today:yyyy-MM-dd}";
        
        var events = _eventStore.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        var dailyMenu = new DailyMenu(today, location);
        
        await foreach (var @event in events)
        {
            var menuEventData = JsonSerializer.Deserialize<IDomainEvent>(
                Encoding.UTF8.GetString(@event.Event.Data.Span));

            switch (menuEventData)
            {
                case MenuItemAddedEvent menuItemAdded:
                    dailyMenu.AddMenuItem(menuItemAdded.MenuItem);
                    break;
                case MenuItemRemovedEvent menuItemRemoved:
                    dailyMenu.RemoveMenuItem(menuItemRemoved.MenuItemId);
                    break;
                case DailyMenuDisabledEvent:
                    dailyMenu.DisableMenu();
                    break;
                case DailyMenuEnabledEvent:
                    dailyMenu.EnableMenu();
                    break;
            }
        }

        return dailyMenu.IsEnabled ? dailyMenu.MenuItems : Enumerable.Empty<MenuItem>();
    }
}