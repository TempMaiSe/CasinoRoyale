using System.Security.Claims;
using System.Text.Encodings.Web;
using EventStore.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CasinoRoyale.Api.Infrastructure.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly EventStoreClient _eventStore;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        EventStoreClient eventStore)
        : base(options, logger, encoder, clock)
    {
        _eventStore = eventStore;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (string.IsNullOrEmpty(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        try
        {
            var result = _eventStore.ReadAllAsync(
                Direction.Forwards,
                Position.Start,
                maxCount: 1000,
                resolveLinkTos: true,
                filterName: "DeviceRegistered");

            await foreach (var @event in result)
            {
                var eventData = JsonSerializer.Deserialize<DeviceRegisteredEvent>(
                    System.Text.Encoding.UTF8.GetString(@event.Event.Data.Span));

                if (eventData?.ApiKey == providedApiKey)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, eventData.DeviceId.ToString()),
                        new Claim(ClaimTypes.Name, eventData.Name),
                        new Claim("DeviceType", eventData.Type.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
            }

            return AuthenticateResult.Fail("Invalid API Key");
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
        }
    }
}