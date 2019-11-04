using Microsoft.Extensions.Caching.Distributed;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace McGuireV10.OrleansDistributedCache
{
    /// <summary>
    /// An IDistributedCache implementation based upon Microsoft Orleans. Expects Dependency Injection 
    /// to provide an Orleans IClusterClient singleton service to the constructor.
    /// </summary>
    public class OrleansDistributedCacheService : IDistributedCache
    {
        public const string OrleansDistributedCacheStorageProviderName = "OrleansDistributedCacheStorageProvider";

        private const string Use_Async_Only_Message = "OrleansDistributedCacheService only supports asynchronous operations";

        private readonly IClusterClient clusterClient;

        public OrleansDistributedCacheService(IClusterClient clusterClient)
        {
            this.clusterClient = clusterClient;
        }

        /// <summary>
        /// Stores a byte array to the cache by updating the State of an Orleans grain identified by the provided key.
        /// </summary>
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var created = DateTimeOffset.UtcNow;
            var expires = AbsoluteExpiration(created, options);
            var seconds = ExpirationSeconds(created, expires, options);
            return clusterClient.GetGrain<IOrleansDistributedCacheGrain<byte[]>>(key).Set(new Immutable<byte[]>(value), TimeSpan.FromSeconds(seconds ?? 0));
        }

        /// <summary>
        /// Retrieves a byte array from the cache by reading the State from an Orleans grain identified by the provided key.
        /// </summary>
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
            => (await clusterClient.GetGrain<IOrleansDistributedCacheGrain<byte[]>>(key).Get()).Value;

        /// <summary>
        /// Unnecessary for an Orleans-based cache. Forces a reload from persistent storage of the State from an Orleans grain identified by the provided key.
        /// </summary>
        public Task RefreshAsync(string key, CancellationToken token = default)
            => clusterClient.GetGrain<IOrleansDistributedCacheGrain<byte[]>>(key).Refresh();

        /// <summary>
        /// Stores a null value in the State of an Orleans grain identified by the provided key. Note this does not actually delete the entry from the underlying storage.
        /// </summary>
        public Task RemoveAsync(string key, CancellationToken token = default)
            => clusterClient.GetGrain<IOrleansDistributedCacheGrain<byte[]>>(key).Clear();

        private static long? ExpirationSeconds(DateTimeOffset creationTime, DateTimeOffset? absoulteExpiration, DistributedCacheEntryOptions options)
        {
            if(absoulteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long)Math.Min(
                    (absoulteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }

            if(absoulteExpiration.HasValue)
                return (long)(absoulteExpiration.Value - creationTime).TotalSeconds;

            if(options.SlidingExpiration.HasValue)
                return (long)options.SlidingExpiration.Value.TotalSeconds;

            return null;
        }

        private static DateTimeOffset? AbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
            => options.AbsoluteExpirationRelativeToNow.HasValue ? creationTime + options.AbsoluteExpirationRelativeToNow : options.AbsoluteExpiration;

        [Obsolete(Use_Async_Only_Message)]
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            => SyncOverAsync.Run(() => SetAsync(key, value, options));

        [Obsolete(Use_Async_Only_Message)]
        public byte[] Get(string key)
            => SyncOverAsync.Run(() => GetAsync(key));

        [Obsolete(Use_Async_Only_Message)]
        public void Refresh(string key)
            => SyncOverAsync.Run(() => RefreshAsync(key));

        [Obsolete(Use_Async_Only_Message)]
        public void Remove(string key)
            => SyncOverAsync.Run(() => RemoveAsync(key));

        private static class SyncOverAsync
        {
            private static readonly TaskFactory factory 
                = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

            public static void Run(Func<Task> task)
                => factory.StartNew(task).Unwrap().GetAwaiter().GetResult();
            
            public static TResult Run<TResult>(Func<Task<TResult>> task)
                => factory.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }
    }
}
