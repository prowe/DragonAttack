
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace Backend
{
    public static class IWebHostExtensions
    {
        public static async Task<IWebHost> StartOrleans(this IWebHost webHost)
        {
            var services = webHost.Services;

            var siloHost = services.GetRequiredService<ISiloHost>();
            await siloHost.StartAsync();

            return webHost;
        }
    }
}