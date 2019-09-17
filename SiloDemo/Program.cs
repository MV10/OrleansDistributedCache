using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using McGuireV10.OrleansDistributedCache;

namespace SiloDemo
{
    class Program
    {
        private static string ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Database=OrleansDistributedCache";
        private static string ClusterId = "SiloDemoCluster";
        private static string ServiceId = "SiloDemo";
        private static string StorageProviderNamespace = "System.Data.SqlClient";

        public static Task Main(string[] args)
        {
            return new HostBuilder()

                .UseOrleans(builder => builder

                    .UseAdoNetClustering(opt =>
                    {
                        opt.ConnectionString = ConnectionString;
                        opt.Invariant = StorageProviderNamespace;
                    })

                    .AddAdoNetGrainStorage(
                        OrleansDistributedCacheService.OrleansDistributedCacheStorageProviderName, 
                        opt =>
                        {
                            opt.ConnectionString = ConnectionString;
                            opt.Invariant = StorageProviderNamespace;
                            opt.UseXmlFormat = true;
                        })

                    .Configure<ClusterOptions>(opt => 
                    {
                        opt.ClusterId = ClusterId;
                        opt.ServiceId = ServiceId;
                    })

                    .Configure<EndpointOptions>(opt => 
                    {
                        opt.AdvertisedIPAddress = IPAddress.Loopback;
                        opt.SiloPort = 11111;
                        opt.GatewayPort = 30000;
                    })

                    .ConfigureApplicationParts(parts => 
                        parts.AddApplicationPart(typeof(OrleansDistributedCacheGrain<>).Assembly).WithReferences()
                    )
                )

                .ConfigureServices(svc =>
                    svc.Configure<ConsoleLifetimeOptions>(opt => opt.SuppressStatusMessages = true)
                )

                .ConfigureLogging(builder => builder.AddConsole())

                .RunConsoleAsync();
        }
    }
}
