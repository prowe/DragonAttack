using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using Orleans.Configuration;
using Dragon.Web;
using Microsoft.AspNetCore.Mvc;
using Orleans.Hosting;

namespace Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(CreateOrleansClient);
            services.AddSingleton(GetGrainFactory);
            services.AddSingleton<IStreamProvider>(GetStreamProvider);
            services.AddSingleton<GameWebSocketHandler>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.Use(app.ApplicationServices.GetService<GameWebSocketHandler>().Handle);
            app.UseMvc();
        }

        private IClusterClient CreateOrleansClient(IServiceProvider services)
        {
            IClusterClient client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "HelloWorldApp";
                })
                .AddSimpleMessageStreamProvider("Default")
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();
            client.Connect().Wait();
            return client;
        }

        private IStreamProvider GetStreamProvider(IServiceProvider services)
        {
            return services.GetRequiredService<IClusterClient>().GetStreamProvider("Default");
        }

        private IGrainFactory GetGrainFactory(IServiceProvider services)
        {
            return services.GetRequiredService<IClusterClient>();
        }
    }
}
