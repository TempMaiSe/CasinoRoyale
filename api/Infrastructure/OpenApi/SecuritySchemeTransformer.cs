using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace CasinoRoyale.Api.Infrastructure.OpenApi;

internal sealed class SecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    
    private readonly IConfiguration _configuration;

    public SecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider, IConfiguration configuration)
    {
        _authenticationSchemeProvider = authenticationSchemeProvider;
        _configuration = configuration;
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>();

        // Add OpenID Connect scheme if present
        if (authenticationSchemes.Any(scheme => scheme.Name == OpenIdConnectDefaults.AuthenticationScheme))
        {
            document.Components.SecuritySchemes["OpenIdConnect"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OpenIdConnect,
                OpenIdConnectUrl = new Uri($"{_configuration["Keycloak:auth-server-url"]}realms/{_configuration["Keycloak:realm"]}/.well-known/openid-configuration"),
                Scheme = "openid"
            };
        }

        // Add API Key scheme if present
        if (authenticationSchemes.Any(scheme => scheme.Name == "ApiKey"))
        {
            document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "X-API-Key",
                In = ParameterLocation.Header,
                Description = "API key for kiosk access"
            };
        }

        // Apply security requirements to all endpoints
        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                operation.Security = [];

                // Add OpenID Connect requirement for admin endpoints
                if (operation.Tags?.Any(t => t.Name.Contains("admin", StringComparison.OrdinalIgnoreCase)) == true)
                {
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [document.Components.SecuritySchemes[OpenIdConnectDefaults.AuthenticationScheme]] = Array.Empty<string>()
                    });
                }

                // Add API Key requirement for kiosk endpoints
                if (operation.Tags?.Any(t => t.Name.Contains("kiosk", StringComparison.OrdinalIgnoreCase)) == true)
                {
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [document.Components.SecuritySchemes["ApiKey"]] = Array.Empty<string>()
                    });
                }
            }
        }
    }
}