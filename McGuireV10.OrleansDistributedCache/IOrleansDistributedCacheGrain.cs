using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace McGuireV10.OrleansDistributedCache
{
    public interface IOrleansDistributedCacheGrain<T> : IGrainWithStringKey
    {
        Task Set(Immutable<T> value, TimeSpan delayDeactivation);
        Task<Immutable<T>> Get();
        Task Refresh();
        Task Clear();
    }
}
