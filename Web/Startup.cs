using System;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Streams;
using Dragon.Web;
using static Orleans.Runtime.Configuration.ClientConfiguration;
using System.Net;
using System.Linq;

namespace Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureOrleans();
            services.AddSingleton(CreateGrainFactory);
            services.AddSingleton<IStreamProvider>(GrainClient.GetStreamProvider("Default"));
            services.AddSingleton<GameWebSocketHandler>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.Use(app.ApplicationServices.GetService<GameWebSocketHandler>().Handle);
            app.UseMvc();
        }

        private void ConfigureOrleans()
        {
            var config = new ClientConfiguration ();
            config.GatewayProvider = GatewayProviderType.Config;

            
            config.Gateways.Add(new IPEndPoint(GetSiloIpAddress(), 40001));

            config.TraceFileName = null;
            config.AddSimpleMessageStreamProvider("Default");
            /*
            string StorageConnectionString  = "DefaultEndpointsProtocol=https;AccountName=prowemarket;AccountKey=4JOmgr/4XmolsEXzQJCrTlgpTqT/GCmwFB78y04sFOw57on+k3V6P36qECUVD86aV6FVBYmrRLvesmydP6jDaw==;";
            var config = new ClientConfiguration();
            config.GatewayProvider = ClientConfiguration.GatewayProviderType.AzureTable;
            config.DeploymentId = Configuration.GetValue<string>("DeploymentId", "dev");
            config.DataConnectionString = StorageConnectionString;
            //config.AddAzureQueueStreamProviderV2("Default", StorageConnectionString);
            */
            GrainClient.Initialize(config);
        }

        private IGrainFactory CreateGrainFactory(IServiceProvider services)
        {
            return GrainClient.GrainFactory;
        }

        private IPAddress GetSiloIpAddress()
        {
            var siloHost = Environment.GetEnvironmentVariable("SiloHost");
            if(siloHost != null)
            {
                Console.WriteLine("Attempting to resolve silo to host: " + siloHost);
                var hostAddresses =  Dns.GetHostAddressesAsync(siloHost).Result;
                Console.WriteLine("Resolved silo host to ", hostAddresses);
                return hostAddresses.First();
            }
            return IPAddress.Loopback;
        }
    }
}
