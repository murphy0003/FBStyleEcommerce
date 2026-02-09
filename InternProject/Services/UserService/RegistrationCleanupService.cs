using InternProject.Data;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Services.UserService
{
    public class RegistrationCleanupService(IServiceScopeFactory scopeFactory,ILogger<RegistrationCleanupService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("RegistrationCleanupService started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredRegistrationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during cleanup.");
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            logger.LogInformation("RegistrationCleanupService stopped.");
        }
        private async Task CleanupExpiredRegistrationsAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var expiredUsers = await context.Users
                .Where(u => !u.IsRegistrationCompleted && u.RegistrationExpiresAt < DateTime.UtcNow)
                .ToListAsync(stoppingToken);
            if (expiredUsers.Count != 0)
            {
                logger.LogInformation("Cleaning up {count} expired registrations", expiredUsers);
                context.Users.Remove(expiredUsers[0]);
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
