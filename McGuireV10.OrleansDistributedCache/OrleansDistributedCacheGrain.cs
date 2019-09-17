using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using System;
using System.Threading.Tasks;

namespace McGuireV10.OrleansDistributedCache
{
    /// <summary>
    /// Stores individual cache values in Orleans silos.
    /// </summary>
    [StorageProvider(ProviderName = OrleansDistributedCacheService.OrleansDistributedCacheStorageProviderName)]
    public class OrleansDistributedCacheGrain<T> : Grain<Immutable<T>>, IOrleansDistributedCacheGrain<T>
    {
        private TimeSpan delayDeactivation = TimeSpan.Zero;

        private readonly OrleansDistributedCacheOptions options;

        public OrleansDistributedCacheGrain(IOptions<OrleansDistributedCacheOptions> options)
        {
            this.options = options.Value;
        }

        /// <summary>
        /// Caches the value with an optional grain deactivation delay (use TimeSpan.Zero to apply the configured default).
        /// </summary>
        public async Task Set(Immutable<T> value, TimeSpan delayDeactivation)
        {
            State = value;
            this.delayDeactivation = (delayDeactivation > TimeSpan.Zero) ? delayDeactivation : options.DefaultDelayDeactivation;

            if (options.PersistWhenSet) 
                await base.WriteStateAsync();

            DelayDeactivation(delayDeactivation);
        }

        /// <summary>
        /// Retrieves the cached value.
        /// </summary>
        public Task<Immutable<T>> Get() 
            => Task.FromResult(State);

        /// <summary>
        /// Updates the cached value from persistent storage.
        /// </summary>
        public async Task Refresh()
        {
            await base.ReadStateAsync();
            DelayDeactivation(delayDeactivation);
        }

        /// <summary>
        /// Resets the cache state to the default (which typically writes a null to the underlying database
        /// storage provider) then sets the grain to deactivate as soon as possible.
        /// </summary>
        public async Task Clear()
        {
            await base.ClearStateAsync();
            DeactivateOnIdle();
        }
    }
}
