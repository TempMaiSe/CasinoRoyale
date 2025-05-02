using KurrentDB.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using NodaTime;
using CasinoRoyale.Api.Application.Commands;
using CasinoRoyale.Api.Application.Queries;
using CasinoRoyale.Api.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;

const string ApiKeyScheme = "ApiKey";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddSingleton<IClock>(NodaTime.SystemClock.Instance);

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
/*options.DocumentTitle = "Casino Royale API";
    options.Version = "v1";
    options.SecuritySchemes.Add("Bearer", new()
    {
        Type = "http",
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.SecuritySchemes.Add("ApiKey", new()
    {
        Type = "apiKey",
        Name = "X-API-Key",
        In = "header"
    });*/
});


// Add Keycloak Authentication
var oidcScheme = OpenIdConnectDefaults.AuthenticationScheme;

builder.Services.AddAuthentication(oidcScheme)
                .AddKeycloakOpenIdConnect(
                    "keycloak", 
                    realm: "casino-royale",
                    oidcScheme,
                    options => 
                    {
                        options.ClientId = "casino-royale";
                        options.ResponseType = OpenIdConnectResponseType.Code;
                        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                        options.SaveTokens = true;
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddCascadingAuthenticationState();

// Add API Key Authentication
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyScheme, _ => { });

// Add Authorization
builder.Services.AddAuthorization();

// Add EventStoreDB
builder.Services.AddSingleton(new KurrentDBClient(KurrentDBClientSettings.Create(
    builder.Configuration.GetConnectionString("EventStore"))));

// Add MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJsApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development, allow any localhost origin due to Aspire's dynamic ports
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // In production, use the configured origins
            policy.WithOrigins(builder.Configuration["AllowedOrigins"].Split(','))
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
// Ensure CORS middleware runs before authentication
app.UseCors("AllowNextJsApp");
app.UseAuthentication();
app.UseAuthorization();

// Menu Management Endpoints
app.MapGet("/api/locations/{locationId}/menu/today", async (Guid locationId, IMediator mediator) =>
    {
        var query = new GetTodayMenuQuery(locationId);
        return await mediator.Send(query);
    })
    .WithName("GetTodayMenu")
    .WithOpenApi();

app.MapGet("/api/menu/dish/{id}", async (Guid id, IMediator mediator) =>
    {
        var query = new GetMenuItemQuery(id);
        var result = await mediator.Send(query);
        return result is null ? Results.NotFound() : Results.Ok(result);
    })
    .WithName("GetMenuItem")
    .WithOpenApi();

app.MapPost("/api/locations/{locationId}/menu/items", async (
    Guid locationId,
    AddMenuItemCommand command,
    IMediator mediator,
    ClaimsPrincipal user) =>
    {
        if (!user.IsInRole("admin"))
            return Results.Forbid();

        if (command.LocationId != locationId)
            return Results.BadRequest("Location ID in URL must match command");

        var result = await mediator.Send(command);
        return Results.Created($"/api/menu/dish/{result}", result);
    })
    .WithName("AddMenuItem")
    .RequireAuthorization()
    .WithOpenApi();

// Device Management Endpoints
app.MapPost("/api/locations/{locationId}/devices", async (
    Guid locationId,
    RegisterDeviceCommand command,
    IMediator mediator,
    ClaimsPrincipal user) =>
    {
        if (!user.IsInRole("admin"))
            return Results.Forbid();

        if (command.LocationId != locationId)
            return Results.BadRequest("Location ID in URL must match command");

        var result = await mediator.Send(command);
        return Results.Created($"/api/devices/{result.DeviceId}", result);
    })
    .WithName("RegisterDevice")
    .RequireAuthorization()
    .WithOpenApi();

app.MapGet("/api/locations/{locationId}/kiosk/today", async (Guid locationId, IMediator mediator) =>
    {
        var query = new GetTodayMenuQuery(locationId);
        return await mediator.Send(query);
    })
    .WithName("GetKioskMenu")
    .RequireAuthorization(ApiKeyScheme)
    .WithOpenApi();

app.MapGet("/api/kiosk/dish/{id}", async (Guid id, IMediator mediator) =>
    {
        var query = new GetMenuItemQuery(id);
        var result = await mediator.Send(query);
        return result is null ? Results.NotFound() : Results.Ok(result);
    })
    .WithName("GetKioskMenuItem")
    .RequireAuthorization(ApiKeyScheme)
    .WithOpenApi();

// Location Management Endpoints
app.MapGet("/api/locations", async (IMediator mediator) =>
    {
        var query = new GetLocationsQuery();
        return await mediator.Send(query);
    })
    .WithName("GetLocations")
    .WithOpenApi();

app.MapPost("/api/locations", async (
    AddLocationCommand command,
    IMediator mediator,
    ClaimsPrincipal user) =>
    {
        if (!user.IsInRole("admin"))
            return Results.Forbid();

        var result = await mediator.Send(command);
        return Results.Created($"/api/locations/{result}", result);
    })
    .WithName("AddLocation")
    .RequireAuthorization()
    .WithOpenApi();

app.MapDefaultEndpoints();

await app.RunAsync();
