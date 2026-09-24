using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var echodb = builder.AddPostgres("postgres").WithDataVolume().AddDatabase("echodb");

var migrations = builder
    .AddProject<Echo_MigrationService>("migrations")
    .WithReference(echodb)
    .WaitFor(echodb);

builder
    .AddProject<Echo_Api>("api")
    .WithReference(echodb)
    .WaitFor(echodb)
    .WaitForCompletion(migrations);

builder.Build().Run();
