using Echo.Data;
using Echo.MigrationService;
using Echo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>(
    "echodb",
    // Migrations rewrite and backfill large tables; the 30 s default would cancel and retry them.
    configureSettings: settings => settings.CommandTimeout = (int)TimeSpan.FromMinutes(30).TotalSeconds,
    configureDbContextOptions: options =>
    {
        options.ConfigureEchoDatabase();
    }
);
builder.Services.AddHostedService<Worker>();
builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

var host = builder.Build();
host.Run();
