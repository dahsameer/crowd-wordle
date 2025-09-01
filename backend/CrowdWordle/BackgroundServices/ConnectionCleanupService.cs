using CrowdWordle.Services;

namespace CrowdWordle.BackgroundServices;

public sealed class ConnectionCleanupService(ConnectionManager connectionManager) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CleanupInterval, stoppingToken);

            var disconnectedUserIds = new List<uint>();

            foreach (var kvp in connectionManager.GetAllConnections())
            {
                if (!kvp.Value.IsOpen)
                {
                    disconnectedUserIds.Add(kvp.Key);
                }
            }

            foreach (var userId in disconnectedUserIds)
            {
                if (connectionManager.GetAllConnections().TryGetValue(userId, out var connection))
                {
                    await connectionManager.CloseConnection(userId, connection);
                }
            }
        }
    }
}