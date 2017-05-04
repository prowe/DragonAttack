using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Streams;

namespace Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureOrleans();
            services.AddSingleton(CreateGrainFactory);
            services.AddSingleton<IStreamProvider>(GrainClient.GetStreamProvider("Default"));
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
            app.UseMvc();
        }

        private void ConfigureOrleans()
        {
            string StorageConnectionString  = "DefaultEndpointsProtocol=https;AccountName=prowemarket;AccountKey=4JOmgr/4XmolsEXzQJCrTlgpTqT/GCmwFB78y04sFOw57on+k3V6P36qECUVD86aV6FVBYmrRLvesmydP6jDaw==;";
            var config = new ClientConfiguration();
            config.GatewayProvider = ClientConfiguration.GatewayProviderType.AzureTable;
            config.DeploymentId = "dev";
            config.DataConnectionString = StorageConnectionString;
            config.TraceFileName = null;
            //config.AddAzureQueueStreamProviderV2("Default", StorageConnectionString);
            config.AddSimpleMessageStreamProvider("Default");
            GrainClient.Initialize(config);
        }

        private IGrainFactory CreateGrainFactory(IServiceProvider services)
        {
            return GrainClient.GrainFactory;
        }
    }
}
