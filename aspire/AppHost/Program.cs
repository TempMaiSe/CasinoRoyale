using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<CasinoRoyale_Api>("api")
                 .WithExternalHttpEndpoints();

builder.AddNpmApp("pwa", workingDirectory: "../../", scriptName: "dev")
       .WithHttpEndpoint(env: "PORT")
       .WithReference(api)
       .WithNpmPackageInstallation()
       .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
