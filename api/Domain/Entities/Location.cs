using NodaTime;

namespace CasinoRoyale.Api.Domain.Entities;

public class Location
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTimeZone TimeZone { get; private set; }
    public bool IsActive { get; private set; }

    private Location() { }

    public Location(string name, DateTimeZone timeZone)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        TimeZone = timeZone;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public LocalDate GetCurrentDate(IClock clock)
    {
        return clock.GetCurrentInstant().InZone(TimeZone).Date;
    }
}