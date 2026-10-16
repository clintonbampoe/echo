using Echo.Data;
using Echo.Seeder;
using Echo.Seeder.Core;
using Echo.ServiceDefaults;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>(
    "echodb",
    configureDbContextOptions: options => options.ConfigureEchoDatabase()
);

// Bind SeederOptions from the "Seeder" config section.
// ValidateDataAnnotations enforces the [Range] attributes at startup.
// Resolution order: CLI args > env vars > appsettings.json > defaults.
//
// CLI example:  dotnet run -- Seeder:Reset=true Seeder:Seed=99
// Env example:  Seeder__Reset=true Seeder__Seed=99
builder
    .Services.AddOptions<SeederOptions>()
    .BindConfiguration(SeederOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// FixedTimeProvider ensures every "now" resolves to ReferenceNow.
// We resolve it from options so the time is consistent with the seed config.
builder.Services.AddSingleton<TimeProvider>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<SeederOptions>>().Value;
    return new FixedTimeProvider(opts.ReferenceNow);
});

builder.Services.AddHostedService<SeederWorker>();

var host = builder.Build();
host.Run();
