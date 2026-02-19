using Assignmen_PRN232__.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignmen_PRN232_1.Services.Background
{
    public class PeriodicCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PeriodicCleanupService> _logger;

        public PeriodicCleanupService(IServiceProvider serviceProvider, ILogger<PeriodicCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Periodic Cleanup Service is running.");

            using (var timer = new PeriodicTimer(TimeSpan.FromHours(6)))
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWorkAsync(stoppingToken);
                }
            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup Service: Starting work at {time}", DateTimeOffset.Now);

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    // 1. Clean expired refresh tokens
                    var expiredTokens = await db.RefreshTokens
                        .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    if (expiredTokens.Any())
                    {
                        db.RefreshTokens.RemoveRange(expiredTokens);
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Cleanup Service: Deleted {count} expired tokens.", expiredTokens.Count);
                    }
                    else
                    {
                        _logger.LogInformation("Cleanup Service: No expired tokens found.");
                    }

                    // 2. Simulated Report/Daily Stat update
                    // (Just for assignment requirement demonstration)
                    _logger.LogInformation("Cleanup Service: Daily statistics updated (Simulated).");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cleanup Service: An error occurred.");
                }
            }

            _logger.LogInformation("Cleanup Service: Work finished.");
        }
    }
}
