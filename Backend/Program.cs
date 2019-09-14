using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

namespace Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost host = CreateWebHostBuilder(args).Build();
            ISiloHost siloHost = host.Services.GetRequiredService<ISiloHost>();
            IClusterClient clusterClient = host.Services.GetRequiredService<IClusterClient>();

            try {
                await siloHost.StartAsync();
                await clusterClient.Connect();
                host.Run();
            } finally {
                await clusterClient.Close();
                await siloHost.StopAsync();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
