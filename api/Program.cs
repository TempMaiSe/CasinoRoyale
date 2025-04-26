using KurrentDB.Client;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using MediatR;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;
using NodaTime;
using CasinoRoyale.Api.Domain.Entities;
using CasinoRoyale.Api.Application.Commands;
using CasinoRoyale.Api.Application.Queries;

const string ApiKeyScheme = "ApiKey";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<IClock>(NodaTime.SystemClock.Instance);

// AddEndpointsApiExplorer requires Microsoft.AspNetCore.Mvc.Versioning
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Casino Royale API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = ParameterLocation.Header
    });
});

// Add Keycloak Authentication
var authenticationOptions = builder.Configuration
    .GetSection(KeycloakAuthenticationOptions.Section)
    .Get<KeycloakAuthenticationOptions>();

builder.Services.AddKeycloakAuthentication(authenticationOptions);

// Add API Key Authentication
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyScheme, _ => { });

// Add Authorization
var authorizationOptions = builder.Configuration
    .GetSection(KeycloakProtectionClientOptions.Section)
    .Get<KeycloakProtectionClientOptions>();

builder.Services.AddKeycloakAuthorization(authorizationOptions);

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
        policy.WithOrigins(builder.Configuration["AllowedOrigins"].Split(','))
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowNextJsApp");
app.UseAuthentication();
app.UseAuthorization();

// Menu Management Endpoints
app.MapGet("/api/menu/today", async (IMediator mediator) =>
    {
        var query = new GetTodayMenuQuery();
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

app.MapPost("/api/menu/items", async (
    AddMenuItemCommand command,
    IMediator mediator,
    ClaimsPrincipal user) =>
    {
        if (!user.IsInRole("admin"))
            return Results.Forbid();

        var result = await mediator.Send(command);
        return Results.Created($"/api/menu/dish/{result}", result);
    })
    .WithName("AddMenuItem")
    .RequireAuthorization()
    .WithOpenApi();

// Device Management Endpoints
app.MapPost("/api/devices", async (
    RegisterDeviceCommand command,
    IMediator mediator,
    ClaimsPrincipal user) =>
    {
        if (!user.IsInRole("admin"))
            return Results.Forbid();

        var result = await mediator.Send(command);
        return Results.Created($"/api/devices/{result.DeviceId}", result);
    })
    .WithName("RegisterDevice")
    .RequireAuthorization()
    .WithOpenApi();

app.MapGet("/api/kiosk/today", async (IMediator mediator) =>
    {
        var query = new GetTodayMenuQuery();
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

await app.RunAsync();
