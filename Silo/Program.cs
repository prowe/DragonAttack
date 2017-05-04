using System;
using System.Net;
using System.Threading;
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
            var assemblyName = typeof(AzureTableStorage).AssemblyQualifiedName;
            Console.WriteLine("Using assembly path" +  assemblyName);

            string StorageConnectionString  = "DefaultEndpointsProtocol=https;AccountName=prowemarket;AccountKey=4JOmgr/4XmolsEXzQJCrTlgpTqT/GCmwFB78y04sFOw57on+k3V6P36qECUVD86aV6FVBYmrRLvesmydP6jDaw==;";
                        var config = new ClusterConfiguration();
            config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.AzureTable;
            config.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;
            config.Globals.MembershipTableAssembly = assemblyName;
            config.Globals.DeploymentId = "dev";
            config.Globals.DataConnectionString = StorageConnectionString;
            config.Defaults.Port = 40000;
            config.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 40001);
            config.Defaults.TraceFileName = null;
            config.Defaults.TraceFilePattern = null;
            config.AddAzureTableStorageProvider(
                providerName: "Default",
                connectionString: StorageConnectionString
            );
            config.AddAzureTableStorageProvider(
                providerName: "PubSubStore",
                connectionString: StorageConnectionString
            );
            config.AddAzureQueueStreamProviderV2("Default", StorageConnectionString);
            return config;
        }
    }
}
