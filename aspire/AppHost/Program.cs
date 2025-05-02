using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var eventStore = builder.AddEventStore("eventstore")
                        .WithImage("kurrentplatform/kurrentdb:25.0")
                        .WithDataVolume()
                        .WithOtlpExporter();

var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .WithDataVolume()
                      .WithOtlpExporter();

var api = builder.AddProject<CasinoRoyale_Api>("api")
                 .WithReference(eventStore)
                 .WithReference(keycloak)
                 .WithExternalHttpEndpoints();

builder.AddNpmApp("pwa", workingDirectory: "../../", scriptName: "dev")
       .WithHttpEndpoint(env: "PORT")
       .WithReference(api)
       .WithReference(keycloak)
       .WithNpmPackageInstallation()
       .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
