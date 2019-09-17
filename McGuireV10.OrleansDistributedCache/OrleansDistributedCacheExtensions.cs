using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace McGuireV10.OrleansDistributedCache
{
    public static class OrleansDistributedCacheExtensions
    {
        /// <summary>
        /// Configures and registers the OrleansDistributedCacheService. You must also register an Orleans IClusterClient service.
        /// </summary>
        public static IServiceCollection AddOrleansDistributedCache(this IServiceCollection services, Action<OrleansDistributedCacheOptions> options = null)
        {
            services.AddOptions();
            services.Configure(options ?? new Action<OrleansDistributedCacheOptions>(defaultOptions => { }));
            services.AddSingleton<IDistributedCache, OrleansDistributedCacheService>();
            return services;
        }
    }
}
