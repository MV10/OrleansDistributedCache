using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using McGuireV10.OrleansDistributedCache;

namespace StandAloneClientDemo
{
    // Run two copies of this to watch them update each other via cache

    public class Program
    {
        private static string ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Database=OrleansDistributedCache";
        private static string ClusterId = "SiloDemoCluster";
        private static string ServiceId = "SiloDemo";
        private static string StorageProviderNamespace = "System.Data.SqlClient";

        private static ServiceProvider services;

        private const int REFRESH_RATE_MS = 250; 

        public static async Task Main(string[] args)
        {
            await Startup();
            await PopulateCache();
            bool running = true;
            while(running)
            {
                await RefreshDisplay();
                if(Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        running = false;
                    }
                    else
                    {
                        if (key.KeyChar >= '0' && key.KeyChar <= '9')
                            await UpdateCacheItem(key.KeyChar);
                    }
                }
                else
                {
                    await Task.Delay(REFRESH_RATE_MS);
                }
            }
            await Shutdown();
        }

        private static async Task Startup()
        {
            var orleansClient = await ConnectToOrleans();

            services = new ServiceCollection()
                .AddSingleton(orleansClient)
                .AddOrleansDistributedCache(opt => opt.PersistWhenSet = true )
                .BuildServiceProvider();
        }

        private static async Task<IClusterClient> ConnectToOrleans()
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

            return orleansClient;
        }

        private static async Task PopulateCache()
        {
            var cache = services.GetRequiredService<IDistributedCache>();
            await cache.SetAsync("data0", new Data("zero").Serialize());
            await cache.SetAsync("data1", new Data("one").Serialize());
            await cache.SetAsync("data2", new Data("two").Serialize());
            await cache.SetAsync("data3", new Data("three").Serialize());
            await cache.SetAsync("data4", new Data("four").Serialize());
            await cache.SetAsync("data5", new Data("five").Serialize());
            await cache.SetAsync("data6", new Data("six").Serialize());
            await cache.SetAsync("data7", new Data("seven").Serialize());
            await cache.SetAsync("data8", new Data("eight").Serialize());
            await cache.SetAsync("data9", new Data("nine").Serialize());
        }

        private static async Task RefreshDisplay()
        {
            var cache = services.GetRequiredService<IDistributedCache>();

            Console.Clear();
            for(int i = 0; i < 10; i++)
            {
                var data = Data.Deserialize(await cache.GetAsync($"data{i}"));
                Console.WriteLine($"{i}: {data.Value}, {data.Timestamp.ToString("o")}");
            }
            Console.WriteLine("\n\nPress 0-9 to update timestamp, or ESC to exit.");
        }

        private static async Task UpdateCacheItem(char key)
        {
            var cache = services.GetRequiredService<IDistributedCache>();
            var data = Data.Deserialize(await cache.GetAsync($"data{key}"));
            data.Timestamp = DateTimeOffset.Now;
            await cache.SetAsync($"data{key}", data.Serialize());
        }

        private static async Task Shutdown()
        {
            var orleansClient = services.GetRequiredService<IClusterClient>();
            await orleansClient.Close();
            await services.DisposeAsync();
        }
    }

    public class Data
    {
        public Data() { }
        public Data(string value) => Value = value;
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
        public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes(this);
        public static Data Deserialize(byte[] buffer) => JsonSerializer.Deserialize<Data>(buffer);
    }
}
