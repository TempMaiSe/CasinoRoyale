using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var eventStore = builder.AddEventStore("eventstore")
                        .WithImage("kurrentplatform/kurrentdb:25.0")
                        .WithOtlpExporter();

var api = builder.AddProject<CasinoRoyale_Api>("api")
                 .WithReference(eventStore)
                 .WithExternalHttpEndpoints();

builder.AddNpmApp("pwa", workingDirectory: "../../", scriptName: "dev")
       .WithHttpEndpoint(env: "PORT")
       .WithReference(api)
       .WithNpmPackageInstallation()
       .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
