using System.Reflection;
using L_Bank_W_Backend.DbAccess;
using L_Bank_W_Backend.DbAccess.EFRepositories;
using L_Bank_W_Backend.DbAccess.Interfaces;
using L_Bank_W_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LBank.Cli2;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "")
            .ConfigureServices(
                (host, services) =>
                {
                    var dbSettings =
                        host.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
                        ?? throw new InvalidOperationException();

                    services.AddHostedService<ConsoleHostedService>();
                    services.AddSingleton<IConfiguration>(host.Configuration);
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options
                            .UseSqlServer(
                                dbSettings.ConnectionString,
                                x => x.MigrationsAssembly("L-Bank.DbAccess")
                            )
                            .UseSeeding(
                                (context, _) =>
                                {
                                    SeedData.Seed(context);
                                }
                            );
                    });

                    services.AddTransient<IEFLedgerRepository, EFLedgerRepository>();
                    services.AddTransient<IEFUserRepository, EFUserRepository>();
                    services.AddTransient<IEFUserRepository, EFUserRepository>();
                }
            )
            .RunConsoleAsync();
    }
}

internal sealed class ConsoleHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public ConsoleHostedService(
        ILogger<ConsoleHostedService> logger,
        IHostApplicationLifetime appLifetime
    )
    {
        _logger = logger;
        _lifetime = appLifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(() =>
        {
            Console.WriteLine("The L-Bank.Web");
            Console.WriteLine();

            Console.ReadKey();
            _lifetime.StopApplication();
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
