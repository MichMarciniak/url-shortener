using UrlShortener.Api.Data;

namespace UrlShortener.Api.Services.Background;

public class CleanupWorker : BackgroundService
{
    private readonly ILogger<CleanupWorker> _logger;
    private readonly IServiceScopeFactory _factory;
    private const int Days = 1;
    private const int DaysThreshold = 30;

    public CleanupWorker(ILogger<CleanupWorker> logger, IServiceScopeFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting db cleanup...");

                using (var scope = _factory.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<IUrlService>();
                    await service.CleanupOldLinksAsync(DaysThreshold);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting urls");
            }

            await Task.Delay(TimeSpan.FromDays(Days), stoppingToken);
        }
    }
}