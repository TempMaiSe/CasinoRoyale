using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Domain.Events;
using NodaTime;

namespace CasinoRoyale.Api.Domain.Aggregates;

public class DailyMenu
{
    public Guid Id { get; private set; }
    public LocalDate Date { get; private set; }
    public bool IsEnabled { get; private set; }
    private readonly List<MenuItem> _menuItems = new();
    public IReadOnlyCollection<MenuItem> MenuItems => _menuItems.AsReadOnly();
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private DailyMenu() { }

    public DailyMenu(LocalDate date)
    {
        Id = Guid.CreateSequential();
        Date = date;
        IsEnabled = true;
        AddDomainEvent(new DailyMenuCreatedEvent(Id, date));
    }

    public void DisableMenu()
    {
        if (!IsEnabled) return;
        IsEnabled = false;
        AddDomainEvent(new DailyMenuDisabledEvent(Id, Date));
    }

    public void EnableMenu()
    {
        if (IsEnabled) return;
        IsEnabled = true;
        AddDomainEvent(new DailyMenuEnabledEvent(Id, Date));
    }

    public void AddMenuItem(MenuItem menuItem)
    {
        if (_menuItems.Any(x => x.Id == menuItem.Id))
            throw new InvalidOperationException($"Menu item with ID {menuItem.Id} already exists");

        _menuItems.Add(menuItem);
        AddDomainEvent(new MenuItemAddedEvent(Id, Date, menuItem));
    }

    public void RemoveMenuItem(Guid menuItemId)
    {
        var menuItem = _menuItems.FirstOrDefault(x => x.Id == menuItemId);
        if (menuItem == null) return;

        _menuItems.Remove(menuItem);
        AddDomainEvent(new MenuItemRemovedEvent(Id, Date, menuItemId));
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}