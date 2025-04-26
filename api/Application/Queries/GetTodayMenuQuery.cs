using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using EventStore.Client;
using MediatR;
using NodaTime;
using System.Text;
using System.Text.Json;

namespace CasinoRoyale.Api.Application.Queries;

public record GetTodayMenuQuery() : IQuery<IEnumerable<MenuItem>>;

public class GetTodayMenuQueryHandler : IRequestHandler<GetTodayMenuQuery, IEnumerable<MenuItem>>
{
    private readonly EventStoreClient _eventStore;
    private readonly IClock _clock;

    public GetTodayMenuQueryHandler(EventStoreClient eventStore, IClock clock)
    {
        _eventStore = eventStore;
        _clock = clock;
    }

    public async Task<IEnumerable<MenuItem>> Handle(GetTodayMenuQuery request, CancellationToken cancellationToken)
    {
        var today = LocalDate.FromDateTime(DateTime.SpecifyKind(_clock.GetCurrentInstant().ToDateTimeUtc(), DateTimeKind.Utc));
        var streamName = $"dailymenu-{today:yyyy-MM-dd}";
        
        var events = _eventStore.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        var dailyMenu = new DailyMenu(today);
        
        await foreach (var @event in events)
        {
            var eventData = JsonSerializer.Deserialize<IDomainEvent>(
                Encoding.UTF8.GetString(@event.Event.Data.Span));

            switch (eventData)
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