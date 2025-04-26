using CasinoRoyale.Api.Domain.Entities;

namespace CasinoRoyale.Api.Application.Commands;

public record AddMenuItemCommand(
    Guid LocationId,
    Guid DailyMenuId,
    string Name,
    string Description,
    decimal EmployeePrice,
    decimal ExternalPrice,
    ICollection<string> Allergens,
    MenuType Type,
    bool IsSpecialOffer = false,
    DayOfWeek? SpecialOfferDay = null) : ICommand<Guid>;