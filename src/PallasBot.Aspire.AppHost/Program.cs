using PallasBot.Aspire.AppHost.Extensions;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.ConfigureAppHost();

#region Parameters

var postgresqlPassword = builder.AddParameter("postgresql-password", "1nyWacUqpb3NMd8BUECiZkP51VHNYaxL", false, true);
var postgresqlTag = builder.AddParameter("postgresql-tag", "17.0").GetString();

var enablePgadmin = builder.AddParameter("enable-pgadmin", "false").GetBool();

#endregion

#region External Services

var postgresql = builder
    .AddResourceWithConnectionString(b =>
    {
        var pg = b
            .AddPostgres("postgresql-instance", password: postgresqlPassword)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithOtlpExporter()
            .WithImageTag(postgresqlTag)
            .WithDataVolume("pallas-bot-postgresql");
        if (enablePgadmin)
        {
            pg.WithPgAdmin(pgadmin => pgadmin
                .WithImageTag("latest")
                .WithLifetime(ContainerLifetime.Persistent));
        }

        pg.AddDatabase("pallas-bot");
        return pg;
    }, "PostgreSQL");

#endregion

builder.AddProject<PallasBot_App_Bot>("bot")
    .WithReference(postgresql)
    .WithHttpsHealthCheck("/health");

var app = builder.Build();

await app.RunAsync();
