using Microsoft.Extensions.Caching.Distributed;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace McGuireV10.OrleansDistributedCache
{
    /// <summary>
    /// An IDistributedCache implementation based upon Microsoft Orleans. Requires an Orleans IClusterClient singleton service.
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

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var created = DateTimeOffset.UtcNow;
            var expires = AbsoluteExpiration(created, options);
            var seconds = ExpirationSeconds(created, expires, options);
            return clusterClient.GetGrain<IOrleansDistributedCacheGrain<byte[]>>(key).Set(new Immutable<byte[]>(value), TimeSpan.FromSeconds(seconds ?? 0));
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
            => (await clusterClient.GetGrain<IOrleansDistributedCacheGrain<byte[]>>(key).Get()).Value;

        public Task RefreshAsync(string key, CancellationToken token = default)
            => clusterClient.GetGrain<IOrleansDistributedCacheGrain<byte[]>>(key).Refresh();

        public Task RemoveAsync(string key, CancellationToken token = default)
            => clusterClient.GetGrain<IOrleansDistributedCacheGrain<byte[]>>(key).Refresh();

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
            => throw new NotImplementedException(Use_Async_Only_Message);

        [Obsolete(Use_Async_Only_Message)]
        public byte[] Get(string key)
            => throw new NotImplementedException(Use_Async_Only_Message);

        [Obsolete(Use_Async_Only_Message)]
        public void Refresh(string key)
            => throw new NotImplementedException(Use_Async_Only_Message);

        [Obsolete(Use_Async_Only_Message)]
        public void Remove(string key)
            => throw new NotImplementedException(Use_Async_Only_Message);
    }
}
