using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Web
{
    public static class ConnectToSiloExtension
    {
        public static async Task<IWebHost> ConnectToSilo(this IWebHost webHost)
        {
            var services = webHost.Services;
            var logger = services.GetRequiredService<ILogger<IWebHost>>();
            var client = services.GetRequiredService<IClusterClient>();
            var attemptsRemaining = 5;
            await client.Connect(async (Exception e) => {
                if (attemptsRemaining <= 0)
                {
                    return false;
                }

                attemptsRemaining--;
                logger.LogWarning($"Failed to connect to cluster. {attemptsRemaining} remaingin: Exception: {e}");
                await Task.Delay(TimeSpan.FromSeconds(4));
                return true;
            });

            logger.LogInformation("Client successfully connect to silo host");
            return webHost;
        }
    }
}