using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using McGuireV10.OrleansDistributedCache;

namespace SessionStateDemo
{
    public class Program
    {
        private static string ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Database=OrleansDistributedCache";
        private static string ClusterId = "SiloDemoCluster";
        private static string ServiceId = "SiloDemo";
        private static string StorageProviderNamespace = "System.Data.SqlClient";

        public static async Task Main(string[] args)
        {
            IClusterClient orleansClient = new ClientBuilder()

                .UseAdoNetClustering(opt => 
                {
                    opt.ConnectionString = ConnectionString;
                    opt.Invariant = StorageProviderNamespace;
                })

                .ConfigureServices(svc =>
                    svc.Configure<ClusterOptions>(opt =>
                    {
                        opt.ClusterId = ClusterId;
                        opt.ServiceId = ServiceId;
                    })
                )

                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(OrleansDistributedCacheGrain<>).Assembly).WithReferences()
                )

                .Build();

            // Warning: not production-ready: requires retry logic, disposing the ref upon failure, etc.
            await orleansClient.Connect().ConfigureAwait(false);

            await Host.CreateDefaultBuilder(args)

                .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())

                .ConfigureServices(svc =>
                {
                    svc.AddSingleton(orleansClient);
                    svc.Configure<ConsoleLifetimeOptions>(opt => opt.SuppressStatusMessages = true);
                })

                .ConfigureLogging(builder => builder.AddConsole())

                .Build()
                .RunAsync();
        }
    }
}
