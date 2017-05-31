using System;
using System.Net;
using System.Threading;
using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using Orleans.Storage;

namespace Silo
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = BuildAzureClusterConfig();
            var siloHost = new SiloHost(Dns.GetHostName(), config);
            siloHost.LoadOrleansConfig();

            siloHost.InitializeOrleansSilo();
            siloHost.StartOrleansSilo(false);

            Thread.Sleep(Timeout.Infinite);
        }

        private static ClusterConfiguration BuildAzureClusterConfig() 
        {   
            string StorageConnectionString  = "DefaultEndpointsProtocol=https;AccountName=prowemarket;AccountKey=4JOmgr/4XmolsEXzQJCrTlgpTqT/GCmwFB78y04sFOw57on+k3V6P36qECUVD86aV6FVBYmrRLvesmydP6jDaw==;";
            var config = new ClusterConfiguration();
            var siloAddress = new IPEndPoint(IPAddress.Loopback, 40000);
            config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.MembershipTableGrain;
            config.Globals.SeedNodes.Add(siloAddress);
            config.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;

            config.Defaults.HostNameOrIPAddress = "localhost";
            config.Defaults.Port = 40000;
            config.Defaults.TraceFileName = null;
            config.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 40001);

            config.PrimaryNode = siloAddress;

            config.AddMemoryStorageProvider(
                providerName: "PubSubStore"
            );

            config.AddSimpleMessageStreamProvider(
                providerName: "Default",
                fireAndForgetDelivery: true
            );
            return config;
        }
    }
}
