namespace CasinoRoyale.Api.Domain.Entities;

public class Device
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string ApiKey { get; private set; }
    public DeviceType Type { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private Device() { }

    public Device(string name, DeviceType type)
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        ApiKey = GenerateApiKey();
        IsEnabled = true;
        RegisteredAt = DateTime.UtcNow;
    }

    private static string GenerateApiKey()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void Enable()
    {
        IsEnabled = true;
    }
}

public enum DeviceType
{
    SingleDish,
    DailyMenu
}