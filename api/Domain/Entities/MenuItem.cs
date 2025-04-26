namespace CasinoRoyale.Api.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal EmployeePrice { get; private set; }
    public decimal ExternalPrice { get; private set; }
    public ICollection<string> Allergens { get; private set; }
    public MenuType Type { get; private set; }
    public bool IsSpecialOffer { get; private set; }
    public DayOfWeek? SpecialOfferDay { get; private set; }

    private MenuItem() { }

    public MenuItem(
        string name,
        string description,
        decimal employeePrice,
        decimal externalPrice,
        ICollection<string> allergens,
        MenuType type,
        bool isSpecialOffer = false,
        DayOfWeek? specialOfferDay = null)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Description = description;
        EmployeePrice = employeePrice;
        ExternalPrice = externalPrice;
        Allergens = allergens;
        Type = type;
        IsSpecialOffer = isSpecialOffer;
        SpecialOfferDay = specialOfferDay;
    }
}