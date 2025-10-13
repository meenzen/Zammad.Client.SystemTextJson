using Zammad.Client;

namespace Zammad.Example;

public class Worker(ILogger<Worker> logger, IZammadClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var user = await client.GetUserMeAsync();
        logger.LogInformation(
            "Signed in as {FirstName} {LastName} ({Email})",
            user.FirstName,
            user.LastName,
            user.Email
        );
    }
}
